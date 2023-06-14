using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using xACME.Models;
using xACME.Models.Acme;

namespace xACME.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class DirectoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DirectoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult OnGet()
        {
            var hostname = "https://" + _configuration["ServiceHostName"] + "/acme/";

            var meta = new MetaDirectoryObject
            {
                caaIdentities = new List<string> { _configuration["MetaDirectory:CaaIdentity"] },
                externalAccountRequired = false,
                termsOfService = _configuration["MetaDirectory:ToS"],
                website = _configuration["MetaDirectory:Website"]
            };

            var directory = new DirectoryMetadata
            {
                keyChange = hostname + "key-change",
                meta = meta,
                newAccount = hostname + "new-acct",
                newNonce = hostname + "new-nonce",
                newOrder = hostname + "new-order",
                revokeCert = hostname + "revoke-cert"
            };

            return Ok(directory);
        }
    }
}