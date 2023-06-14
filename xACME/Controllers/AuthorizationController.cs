using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using xACME.Helpers;
using xACME.Models.Acme;
using xACME.Models.DbContexts;
using xACME.Models.DbModels;
using xACME.Models.PostAsGet;

namespace xACME.Controllers
{
    [Route("acme/authz")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly AcmeContext _context;
        private readonly IConfiguration _configuration;

        public AuthorizationController(AcmeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("{id}", Name = "OnAuthZPost")]
        [ServiceFilter(typeof(JwsVerify))]
        public async Task<IActionResult> OnAuthZPost([FromRoute] string id, [FromBody] AuthorizationResponse authzRequest)
        {
            var authz = await _context.Authorizations.Where(x => x.Id == Guid.Parse(id)).Include(x => x.Challenges).FirstOrDefaultAsync();

            if (authz == null) return NotFound();
            if (authzRequest == null || authzRequest.status != "deactivated") return Ok(authz.GetAuthorizationResponse(_configuration["ServiceHostName"]));

            var deactivateDbAuthZ = await DeactivateAuthorization(authz);
            return Ok(deactivateDbAuthZ.GetAuthorizationResponse(_configuration["ServiceHostName"]));
        }

        private async Task<DbAuthZ> DeactivateAuthorization(DbAuthZ authorization)
        {
            var account = (await _context.Orders.Include(x => x.ReuqestAccount)
                .Where(x => x.Authorizations.Contains(authorization)).FirstOrDefaultAsync()).ReuqestAccount;

            var protectedObject = (ProtectedObject)Request.HttpContext.Items["protectedObject"];

            if (protectedObject.GetAccountId() != account.Id)
            {
                throw new Exception("Deactivation request does not come from original requester");
            }

            _context.Update(authorization);
            authorization.Status = AuthZStatus.deactivated;
            await _context.SaveChangesAsync();
            return authorization;
        }
    }
}