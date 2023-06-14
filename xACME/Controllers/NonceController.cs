using Microsoft.AspNetCore.Mvc;
using xACME.Helpers;

namespace xACME.Controllers
{
    [Route("acme/new-nonce")]
    [ApiController]
    public class NonceController : ControllerBase
    {
        [HttpHead]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public IActionResult OnHead()
        {
            Response.Headers.Add("Replay-Nonce", NonceHelper.GetNewNonce());

            return NoContent();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public IActionResult OnGet()
        {
            Response.Headers.Add("Replay-Nonce", NonceHelper.GetNewNonce());

            return NoContent();
        }
    }
}