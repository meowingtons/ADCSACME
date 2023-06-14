using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Jose;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using xACME.Models.Acme;
using xACME.Models.DbContexts;
using xACME.Models.PostAsGet;

namespace xACME.Helpers
{
    public class JwsVerify : Attribute, IResourceFilter
    {
        private readonly AcmeContext _context;

        public JwsVerify(AcmeContext guid) { _context = guid; }

        public void OnResourceExecuted(ResourceExecutedContext context) { }

        public async void OnResourceExecuting(ResourceExecutingContext context)
        {
            var originalContent = new StreamReader(context.HttpContext.Request.Body).ReadToEnd();
            var dataSource = JsonConvert.DeserializeObject<JwsObject>(originalContent);
            var protectedObject = dataSource.GetSerializedProtectedObject();
            context.HttpContext.Response.Headers.Add("Replay-Nonce", NonceHelper.GetNewNonce());

            //check if jwt url matches the servers expected url
            if (!string.Equals(protectedObject.url, context.HttpContext.Request.GetDisplayUrl(), StringComparison.CurrentCultureIgnoreCase))
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:InvalidUrl",
                    Description = "url attribute in Jwt does not match Url requested from server. " + protectedObject.url + " " + context.HttpContext.Request.GetDisplayUrl()
                };
                context.Result = new BadRequestObjectResult(error);
                return;
            }

            //verify nonce is valid
            if (!NonceHelper.IsValidNonce(protectedObject.nonce))
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:badNonce",
                    Description = "The client sent an unacceptable anti-replay nonce"
                };
                context.Result = new BadRequestObjectResult(error);
                return;
            }

            //reject requests where jwk and kid both exist
            if (protectedObject.jwk != null && protectedObject.kid != null)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:jwkAndKidPresent",
                    Description = "Both the Jwk and Kid attributes were present in the request. Only one is allowed."
                };
                context.Result = new BadRequestObjectResult(error);
                return;
            }

            //decode request using ECC key
            object key;

            //jwk is only allowed for new-acct or revokeCert requests. Currently revokes are unsupported.
            if (protectedObject.jwk != null && protectedObject.url.Contains("new-acct"))
            {
                key = protectedObject.jwk.GetCngKey();
            }
            else
            {
                var account = _context.Accounts.Include(query => query.Key).FirstOrDefault(query => query.Id == protectedObject.GetAccountId());

                //if we can't find the account (eg. deleted on server or deactivate and client still thinks it's valid), throw the appropriate error
                if (account == null)
                {
                    var error = new Error
                    {
                        Type = "urn:ietf:params:acme:error:accountDoesNotExist",
                        Description = "The request specified an account that does not exist"
                    };
                    context.Result = new NotFoundObjectResult(error);
                    return;
                }

                key = account.Key.GetCngKey();
            }
            
            //attempt to verify the signature of the request and decode the payload
            string jwt;
            try
            {
                Enum.TryParse(protectedObject.alg, true, out JwsAlgorithm alg);
                jwt = JWT.Decode(dataSource.GetJwtFormat(), key, alg);
            }
            catch (IntegrityException)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:signatureValidationFailed",
                    Description = "Jws signature validation failed"
                };
                context.Result = new BadRequestObjectResult(error);
                return;
            }

            context.HttpContext.Items.Add("originalRequest", originalContent);
            context.HttpContext.Items.Add("protectedObject", protectedObject);

            context.HttpContext.Request.Body =
                await new StringContent(jwt, Encoding.UTF8, "application/json").ReadAsStreamAsync();
        }
    }
}
