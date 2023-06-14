using System;
using System.Security.Cryptography;
using Jose;
using Newtonsoft.Json;
using Security.Cryptography;

namespace xACME.Models.DbModels
{
    public class DbAccountKey
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        //DO NOT EDIT ORDER OR NAME. Hashing function requires the following order/names/case or else it will fail.
        public string crv { get; set; }
        public string e { get; set; }
        public string kty { get; set; }
        public string n { get; set; }
        public string x { get; set; }
        public string y { get; set; }

        public CngKey GetEccKey()
        {
            var x = Base64Url.Decode(this.x);
            var y = Base64Url.Decode(this.y);

            return EccKey.New(x, y);
        }

        public RSA GetRsaKey()
        {
            var eDecoded = Base64Url.Decode(this.e);
            var nDecoded = Base64Url.Decode(this.n);

            return new RSACng(RsaKey.New(eDecoded, nDecoded));
        }

        public object GetCngKey()
        {
            if (kty.Equals("RSA"))
            {
                return GetRsaKey();
            }

            if (kty == "EC")
            {
                return GetEccKey();
            }

            throw new Exception("Unrecognized Key Type");
        }
    }
}
