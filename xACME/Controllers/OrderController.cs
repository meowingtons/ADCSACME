using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using xACME.Helpers;
using xACME.Models.Acme;
using xACME.Models.DbContexts;
using xACME.Models.DbModels;
using xACME.Models.PostAsGet;

namespace xACME.Controllers
{
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AcmeContext _context;
        private readonly IConfiguration _configuration;

        public OrderController(AcmeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("acme/new-order")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnPost([FromBody] OrderRequest order)
        {
            //verify identifier types are valid and that they don't contain a wildcard
            //TODO add support for wildcards
            if (order.Identifiers.Count(x => x.Type != AuthZIdentifierType.dns) != 0 && order.Identifiers.Exists(x => x.value.Contains("*")))
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:rejectedIdentifier",
                    Description = "The server will not issue for the identifier"
                };
                return BadRequest(error);
            }

            //verify validity period matches config file or is null, so we can supply it
            //TODO actually check the config file for validity period
            if (!order.IsValidityPeriodAcceptable(90) || order.NotBefore != null && order.NotAfter != null)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:rejectedIdentifier",
                    Description = "The validity period is invalid"
                };
                return BadRequest(error);
            }

            //verify the number of identifiers is below 50 (arbitrary number, but it seems safe)
            //https://social.technet.microsoft.com/wiki/contents/articles/3306.pki-faq-what-is-the-maximum-number-of-names-that-can-be-included-in-the-san-extension.aspx
            if (order.Identifiers.Count > 50)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:rejectedIdentifier",
                    Description = "No more than 50 identifiers allowed"
                };
                return BadRequest(error);
            }

            var originalContent = Request.HttpContext.Items["originalRequest"].ToString();
            var dataSource = JsonConvert.DeserializeObject<JwsObject>(originalContent);
            var protectedObject = dataSource.GetSerializedProtectedObject();

            var expiration = DateTime.Now.AddDays(14);
            var authorizations = new List<DbAuthZ>();

            foreach (var identifier in order.Identifiers)
            {
                //generate challenges for this identifier
                var httpChallenge = new DbChallenge
                {
                    Status = ChallengeStatus.pending,
                    Url = identifier.value,
                    Type = "http-01",
                    Token = NonceHelper.GetNewNonce(),
                    Id =  new Guid()
                };

                var dnsChallenge = new DbChallenge
                {
                    Status = ChallengeStatus.pending,
                    Url = identifier.value,
                    Type = "dns-01",
                    Token = NonceHelper.GetNewNonce(),
                    Id = new Guid()
                };

                var authZ = new DbAuthZ
                {
                    Id = new Guid(),
                    Challenges = new List<DbChallenge> {httpChallenge, dnsChallenge},
                    Expires = expiration,
                    Identifier = identifier,
                    Status = AuthZStatus.pending,
                    Wildcard = false
                };

                authorizations.Add(authZ);
            }

            var response = new DbOrder
            {
                Status = AuthZStatus.pending,
                Expires = expiration,
                //NotBefore = order.ParsedNotBefore,
                //NotAfter = order.ParsedNotAfter,
                Identifiers = order.Identifiers,
                Authorizations = authorizations,
                ReuqestAccount = await _context.Accounts.FindAsync(protectedObject.GetAccountId())
            };

            _context.Orders.Add(response);
            await _context.SaveChangesAsync();
            
            return Created(response.GetUrl(_configuration["ServiceHostName"]), response.GetOrderResponse(_configuration["ServiceHostName"]));
        }

        [HttpPost("{id}", Name = "OnOrderPost")]
        [Route("acme/order/{id}")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnOrderPost([FromRoute] string id)
        {
            var order = await _context.Orders.Where(x => x.Id == Guid.Parse(id)).Include(x => x.Authorizations).FirstOrDefaultAsync();

            return order == null ? NotFound() : (IActionResult)Ok(order.GetOrderResponse(_configuration["ServiceHostName"]));
        }

        [HttpPost("{id}", Name = "OnFinalizePost")]
        [Route("acme/order/{id}/finalize")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnFinalizePost([FromRoute] string id, [FromBody] OrderFinalizeRequest orderRequest)
        {
            var order = await _context.Orders.Include(x => x.Authorizations).Where(x => x.Id == Guid.Parse(id)).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }
            
            //check that order is ready
            if (order.Status != AuthZStatus.ready)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:badCSR",
                    Description = "The order status is not 'ready'"
                };
                return BadRequest(error);
            }

            var identifierDifferences = orderRequest.Identifiers.Except(order.Identifiers.Select(x => x.value)).ToList();

            //check that the CSR contains the same identifiers as the order
            if (identifierDifferences.Count() != 0)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:badCSR",
                    Description = "The server will not issue for the identifier: " + identifierDifferences.ToList()[0]
                };
                return BadRequest(error);
            }

            //verify that the finalize request came from the account that submitted the order
            if (order.ReuqestAccount.Id.ToString() !=
                ((ProtectedObject) Request.HttpContext.Items["protectedObject"]).GetAccountId().ToString())
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:badCSR",
                    Description = "This order was not requested by the account used to submit the request"
                };
                return BadRequest(error);
            }

            _context.Update(order);
            var Adcs = new Adcs(new Adcs.CertificationAuthority(_configuration["AdcsConfig:Hostname"], _configuration["AdcsConfig:Name"]));
            order.RequestId = Adcs.SendCertificateRequest(new Adcs.CertificateRequest(orderRequest.CsrDecodedString, "CertificateTemplate:" + _configuration["AdcsConfig:CertTemplateName"]));
            order.Status = AuthZStatus.valid;
            order.Certificate = order.GetCertificateUrl(_configuration["ServiceHostName"]);
            var result = order.GetOrderResponse(_configuration["ServiceHostName"]);
            await _context.SaveChangesAsync();

            return Ok(result);
        }

        [HttpPost("{id}", Name = "OnCertificatePost")]
        [Route("acme/order/{id}/cert")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnCertificatePost([FromRoute] string id)
        {
            var order = await _context.Orders.Where(x => x.Id == Guid.Parse(id)).Include(x => x.Authorizations).FirstOrDefaultAsync();

            return order == null ? NotFound() : (IActionResult)Ok(CertificateHelper.GetPemChainResponse(_configuration, order.RequestId));
        }
    }
}