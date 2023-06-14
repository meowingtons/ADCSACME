using System;
using System.Text.RegularExpressions;
using xACME.Models.DbModels;

namespace xACME.Models.PostAsGet
{
    public class ProtectedObject
    {
        public string alg { get; set; }
        public string kid { get; set; }
        public DbAccountKey jwk { get; set; }
        public string nonce { get; set; }
        public string url { get; set; }

        public Guid GetAccountId()
        {
            return Guid.Parse(Regex.Replace(kid, ".*\\/acme\\/acct\\/", "", RegexOptions.IgnoreCase));
        }
    }
}