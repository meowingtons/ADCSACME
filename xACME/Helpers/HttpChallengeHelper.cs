using System;
using System.Net.Http;
using System.Threading.Tasks;
using xACME.Models.DbModels;

namespace xACME.Helpers
{
    public class HttpChallengeHelper
    {
        static HttpClient client = new HttpClient();

        public static async Task<bool> VerifyChallenge(DbChallenge challenge, DbAccountKey key, string hostName)
        {
            var uri = "http://" + hostName + ".well-known/acme-challenge/" + challenge.Id;

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody == KeyAuthZHelper.GetHttpKeyAuthZ(key, challenge);
        }
    }
}
