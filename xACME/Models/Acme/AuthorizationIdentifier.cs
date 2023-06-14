using System.Collections.Generic;

namespace xACME.Models.Acme
{
    public class AuthorizationIdentifier
    {
        public AuthZIdentifierType Type { get; set; }
        public string value { get; set; }

        public static List<AuthorizationIdentifier> AuthZDbParser(List<string> ids)
        {
            var list = new List<AuthorizationIdentifier>();

            foreach (var id in ids)
            {
                list.Add(new AuthorizationIdentifier
                {
                    Type = AuthZIdentifierType.dns,
                    value = id
                });
            }

            return list;
        }

        public AuthorizationIdentifierResponse GetAuthorizationIdentifierResponse()
        {
            return new AuthorizationIdentifierResponse()
            {
                Type = Type.ToString(),
                Value = value
            };
        }
    }
}