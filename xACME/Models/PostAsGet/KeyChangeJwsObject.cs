using System;
using System.Text;
using Jose;
using Newtonsoft.Json;
using Security.Cryptography;
using xACME.Models.Acme;
using xACME.Models.DbModels;

// ReSharper disable InconsistentNaming

namespace xACME.Models.PostAsGet
{
    public class KeyChangeJwsObject
    {
        public KeyChangeJwsObject(JwsObject value)
        {
            OuterProtected = value.GetSerializedProtectedObject();
            InnerJwsObject = JsonConvert.DeserializeObject<JwsObject>(Encoding.UTF8.GetString(Base64Url.Decode(value.Payload)));
            OuterSignature = value.Signature;
        }

        private KeyChangeInnerPayload _innerPayloadParsed { get; set; }

        public ProtectedObject OuterProtected { get; set; }
        public JwsObject InnerJwsObject { get; set; }
        public string OuterSignature { get; set; }

        public ProtectedObject InnerProtectedParsed => InnerJwsObject.GetSerializedProtectedObject();
        public KeyChangeInnerPayload InnerPayloadParsed => _innerPayloadParsed;

        public bool InnerPayloadVerified()
        {
            var result = true;

            try
            {

                //check for verification
                var key = InnerProtectedParsed.jwk.GetCngKey();
                var format = InnerJwsObject.GetJwtFormat();
                Enum.TryParse(InnerProtectedParsed.alg, true, out JwsAlgorithm alg);
                var jwt = JWT.Decode(format, key, alg);

                //check that payload is well formed
                _innerPayloadParsed = JsonConvert.DeserializeObject<KeyChangeInnerPayload>(jwt);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public bool UrlMatches()
        {
            return OuterProtected.url == InnerProtectedParsed.url;
        }

        public bool AccountMatches()
        {
            return InnerPayloadParsed.account == OuterProtected.kid;
        }

        public bool OldKeyMatches(DbAccountKey key)
        {
            switch (key.kty)
            {
                case "RSA":
                    return key.n == InnerPayloadParsed.oldKey.n;
                case "EC":
                    return key.x == InnerPayloadParsed.oldKey.x && key.y == InnerPayloadParsed.oldKey.y;
                default:
                    return false;
            }
        }
    }
}
