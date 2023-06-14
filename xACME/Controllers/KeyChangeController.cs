using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using xACME.Helpers;
using xACME.Models.Acme;
using xACME.Models.DbContexts;
using xACME.Models.PostAsGet;

namespace xACME.Controllers
{
    [Route("acme/key-change")]
    [ApiController]
    public class KeyChangeController : ControllerBase
    {
        private readonly AcmeContext _context;
        private readonly IConfiguration _configuration;

        public KeyChangeController(AcmeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnPost()
        {
            var keyChange = new KeyChangeJwsObject(JsonConvert.DeserializeObject<JwsObject>(Request.HttpContext.Items["originalRequest"].ToString()));

            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == keyChange.OuterProtected.GetAccountId());

            //check if account exists and is valid
            if (account == null || account.Status =="deactivated")
            {
                var error = new Error
                {
                    Description = "The request specified an account that does not exist",
                    Type = "urn:ietf:params:acme:error:accountDoesNotExist"
                };
                return NotFound(error);
            }

            //check that inner payload has jwk, check that the payload verifies, check that the payload is a well formed key change object
            if (!keyChange.InnerPayloadVerified())
            {
                var error = new Error
                {
                    Description = "The request message was malformed",
                    Type = "urn:ietf:params:acme:error:malformed"
                };
                return BadRequest(error);
            }

            //check that url parameters of inner and outer jws match
            if (!keyChange.UrlMatches())
            {
                var error = new Error
                {
                    Description = "The outer Jws Url parameter and in the inner Jws Url parameter do not match",
                    Type = "urn:ietf:params:acme:error:UrlMatchFailure"
                };
                return BadRequest(error);
            }

            //check that inner payload account and outer kid match
            if (!keyChange.AccountMatches())
            {
                var error = new Error
                {
                    Description = "The outer Jws kid parameter and in the inner Jws account parameter do not match",
                    Type = "urn:ietf:params:acme:error:AccountMatchFailure"
                };
                return BadRequest(error);
            }

            //check that oldKey matches what's stored in the db
            if (!keyChange.OldKeyMatches(account.Key))
            {
                var error = new Error
                {
                    Description = "The oldKey parameter does not match the server",
                    Type = "urn:ietf:params:acme:error:oldKeyMatchError"
                };
                return BadRequest(error);
            }

            //check that no other account exists with this key
            var key = await _context.AccountKeys.FirstOrDefaultAsync(x =>
                                x.x == keyChange.InnerProtectedParsed.jwk.x && x.y == keyChange.InnerProtectedParsed.jwk.y  && x.x != null && x.y != null || x.n == keyChange.InnerProtectedParsed.jwk.n && x.n != null);
            
            //if key exists, return HTTP 409 and location header.
            if (key != null)
            {
                var existingAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Key == key);
                Response.Headers.Add("Location", "https://" + _configuration["ServiceHostName"] + "/acme/acct/" + existingAccount.Id);
                return Conflict();
            }

            //if all the verifications pass, update the key in the db
            _context.Update(account.Key);
            account.Key.x = keyChange.InnerProtectedParsed.jwk.x;
            account.Key.e = keyChange.InnerProtectedParsed.jwk.e;
            account.Key.kty = keyChange.InnerProtectedParsed.jwk.kty;
            account.Key.n = keyChange.InnerProtectedParsed.jwk.n;
            account.Key.y = keyChange.InnerProtectedParsed.jwk.y;
            account.Key.crv = keyChange.InnerProtectedParsed.jwk.crv;
            await _context.SaveChangesAsync();

            return Ok(account);
        }
    }
}