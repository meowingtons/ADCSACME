using System.Security.Cryptography;
using System.Text;
using Jose;
using Newtonsoft.Json;
using xACME.Models.DbModels;

namespace xACME.Helpers
{
    public class KeyAuthZHelper
    {
        public static string GetKeyThumbprint(DbAccountKey key)
        {
            var publicKey = JsonConvert.SerializeObject(key);
            var thumbprint = Base64Url.Encode(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(publicKey)));

            return thumbprint;
        }

        public static string GetHttpKeyAuthZ(DbAccountKey key, DbChallenge challenge)
        {
            var tp = GetKeyThumbprint(key);
            
            return challenge.Token + "." + tp;
        }

        public static string GetDnsKeyAuthZ(DbAccountKey key, DbChallenge challenge)
        {
            return Base64Url.Encode(Encoding.UTF8.GetBytes(GetHttpKeyAuthZ(key, challenge)));
        }
    }
}
