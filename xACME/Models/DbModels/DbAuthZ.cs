using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using xACME.Models.Acme;

namespace xACME.Models.DbModels
{
    public class DbAuthZ
    {
        public Guid Id { get; set; }
        public AuthorizationIdentifier Identifier { get; set; }
        public AuthZStatus Status { get; set; }
        public DateTime Expires { get; set; }
        public List<DbChallenge> Challenges { get; set; }
        public bool Wildcard { get; set; }

        public AuthorizationResponse GetAuthorizationResponse(string serviceHostName) => new AuthorizationResponse
        {
            status = Status.ToString(),
            expires = XmlConvert.ToString(Expires, XmlDateTimeSerializationMode.Utc),
            identifier = Identifier.GetAuthorizationIdentifierResponse(),
            challenges = Challenges.Select(x => x.GetChallengeResponse(serviceHostName)).ToList(),
            wildcard = Wildcard
        };

        public string GetUrl(string serviceHostName)
        {
            return "https://" + serviceHostName + "/acme/authz/" + Id;
        }
    }
}
