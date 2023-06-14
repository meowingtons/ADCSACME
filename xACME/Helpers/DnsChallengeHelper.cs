using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DnsClient;
using xACME.Models.DbModels;

namespace xACME.Helpers
{
    public class DnsChallengeHelper
    {
        public static async Task<bool> VerifyChallenge(DbChallenge challenge, DbAccountKey key, string hostName)
        {
            var client = new LookupClient();
            client.UseCache = false;

            var response = await client.QueryAsync("_acme-challenge. " + hostName, QueryType.TXT);
            return response.Answers.Count(x => x.ToString() == KeyAuthZHelper.GetDnsKeyAuthZ(key, challenge)) == 1;
        }
    }
}
