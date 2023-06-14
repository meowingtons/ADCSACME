using System;
using System.Text;
using System.Threading.Tasks;
using Jose;
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
    public class AccountController : ControllerBase
    {
        private readonly AcmeContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(AcmeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Route("acme/new-acct")]
        [HttpPost]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> Post([FromBody] Account accountRequest)
        {
            var protectedObject = (ProtectedObject)Request.HttpContext.Items["protectedObject"];

            //check if existing key already exists
            var accountKey = await _context.AccountKeys.FirstOrDefaultAsync(x => x.x == protectedObject.jwk.x && x.y == protectedObject.jwk.y);
            if (accountKey != null)
            {
                //if key exists, return account associated with it
                var existingAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Key == accountKey);
                if (existingAccount != null)
                {
                    Response.Headers.Add("Location", "https://" + _configuration["ServiceHostName"] + "/acme/acct/" + existingAccount.Id);
                    return Ok(existingAccount);
                }
            }
            if (accountRequest.onlyReturnExisting)
            {
                var error = new Error
                {
                    Type = "urn:ietf:params:acme:error:accountDoesNotExist",
                    Description = "The request specified an account that does not exist"
                };
                return BadRequest(error);
            }

            var account = new DbAccount
            {
                Status = ChallengeStatus.valid.ToString(),
                Id = Guid.NewGuid(),
                Contact = accountRequest.contact,
                CreatedAt = DateTime.Now,
                Key = protectedObject.jwk
            };

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return Created("https://" + _configuration["ServiceHostName"] + "/acme/acct/" + account.Id, account);
        }

        [HttpPost]
        [Route("acme/acct/{id}")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> Post([FromRoute] Guid id, [FromBody] Account account)
        {
            var originalAccount = await _context.Accounts.FindAsync(id);

            if (account == null) return Ok(originalAccount);

            _context.Accounts.Update(originalAccount);
            originalAccount.Contact = account.contact;

            //support account deactivation
            if (account.status == "deactivated")
            {
                originalAccount.Status = "deactivated";
            }

            await _context.SaveChangesAsync();

            return Ok(originalAccount);
        }
    }
}