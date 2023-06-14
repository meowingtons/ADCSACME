using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using xACME.Helpers;
using xACME.Models.Acme;
using xACME.Models.DbContexts;

namespace xACME.Controllers
{
    [ApiController]
    [Route("acme/chall")]
    public class ChallengeController : ControllerBase
    {
        private AcmeContext _context;
        private readonly IConfiguration _configuration;

        public ChallengeController(AcmeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("{id}", Name = "OnChallengePost")]
        public async Task<IActionResult> OnChallengePost([FromRoute] string id)
        {
            var order = await _context.Orders.Include(x => x.ReuqestAccount.Key).Include(x => x.Authorizations).Where(x =>
                x.Authorizations.Any(y => y.Challenges.Any(z => z.Id == Guid.Parse(id)))).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges.FindAsync(Guid.Parse(id));
            var authz = order.Authorizations.FirstOrDefault(x => x.Challenges.Contains(challenge));
            var challengeResult = false;

            switch (challenge.Type)
            {
                case "http-01":
                    challengeResult = await HttpChallengeHelper.VerifyChallenge(challenge, order.ReuqestAccount.Key, authz.Identifier.value);
                    break;
                case "dns-01":
                    challengeResult = await DnsChallengeHelper.VerifyChallenge(challenge, order.ReuqestAccount.Key,
                        authz.Identifier.value);
                    break;
            }

            _context.Authorizations.Update(authz);
            _context.Challenges.Update(challenge);
            _context.Orders.Update(order);

            if (challengeResult == false)
            {
                challenge.Status = ChallengeStatus.invalid; //challenge validation failed, so it's set to invalid
                authz.Status = AuthZStatus.invalid; //authz is invalid because challenge moved to invalid
                order.Status = AuthZStatus.invalid; //order is invalid because challenge is invalid
            }
            else
            {
                authz.Status = AuthZStatus.valid;
                challenge.Status = ChallengeStatus.valid;

                //if all the authorizations are valid, set the order object to ready to final validation
                if (order.Authorizations.All(x => x.Status == AuthZStatus.valid))
                {
                    order.Status = AuthZStatus.ready;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(challenge.GetChallengeResponse(_configuration["ServiceHostName"]));
        }
    }
}