using System.Text;
using Jose;
using Newtonsoft.Json;

namespace xACME.Models.PostAsGet
{
    public class JwsObject
    {
        public string Protected { get; set; }
        public string Payload { get; set; }
        public string Signature { get; set; }

        public string GetJwtFormat() => Protected + "." + Payload + "." + Signature;

        public ProtectedObject GetSerializedProtectedObject() =>
            JsonConvert.DeserializeObject<ProtectedObject>(Encoding.UTF8.GetString(Base64Url.Decode(Protected)));
    }
}