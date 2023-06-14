using System.Collections.Generic;
using System.Security.Cryptography;
using Jose;

namespace xACME.Helpers
{
    public class NonceHelper
    {
        private static readonly List<string> NonceList = new List<string>();

        public static string GetNewNonce()
        {
            var bytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);

            var nonce = Base64Url.Encode(bytes);

            NonceList.Add(nonce);

            return nonce;
        }

        public static bool IsValidNonce(string nonce)
        {
            if (!NonceList.Contains(nonce)) return false;
            NonceList.Remove(nonce);

            return true;
        }
    }
}
