using System.Collections.Generic;

namespace xACME.Models.Acme
{
    public class AuthorizationResponse
    {
        public AuthorizationIdentifierResponse identifier { get; set; }
        public string status { get; set; }
        public string expires { get; set; }
        public List<ChallengeResponse> challenges { get; set; }
        public bool wildcard { get; set; }
    }
}