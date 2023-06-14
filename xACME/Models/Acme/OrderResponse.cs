using System.Collections.Generic;

namespace xACME.Models.Acme
{
    public class OrderResponse
    {
        public string status { get; set; }
        public string expires { get; set; }
        public string notBefore { get; set; }
        public string notAfter { get; set; }
        public List<AuthorizationIdentifierResponse> identifiers { get; set; }
        public List<string> authorizations { get; set; }
        public string finalize { get; set; }
        public string certificate { get; set; }
    }
}