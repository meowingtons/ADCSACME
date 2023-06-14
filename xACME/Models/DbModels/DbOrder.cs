using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using xACME.Models.Acme;

namespace xACME.Models.DbModels
{
    public class DbOrder
    {
        public Guid Id { get; set; }
        public DbAccount ReuqestAccount { get; set; }
        public AuthZStatus Status { get; set; }
        public DateTime Expires { get; set; }
        public List<AuthorizationIdentifier> Identifiers { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
        public List<DbAuthZ> Authorizations { get; set; }
        public string Finalize(string serviceHostName) => "https://" + serviceHostName + "/acme/order/" + Id + "/finalize";
        public string Certificate { get; set; }

        public int RequestId { get; set; }

        public string GetUrl(string serviceHostName) => "https://" + serviceHostName + "/acme/order/" + Id;

        public string GetCertificateUrl(string serviceHostName) =>
            "https://" + serviceHostName + "/acme/order/" + Id + "/cert";

        public OrderResponse GetOrderResponse(string serviceHostName = null)
        {
            string notBefore = null;
            string notAfter = null;
            string hostname = null;
            string certificateUrl = null;

            if (NotBefore != DateTime.MinValue)
            {
                notBefore = XmlConvert.ToString(NotBefore, XmlDateTimeSerializationMode.Utc);
            }

            if (NotAfter != DateTime.MinValue)
            {
                notAfter = XmlConvert.ToString(NotAfter, XmlDateTimeSerializationMode.Utc);
            }

            if (serviceHostName != null)
            {
                hostname = Finalize(serviceHostName);
            }

            if (serviceHostName != null && Status == AuthZStatus.valid)
            {
                certificateUrl = GetCertificateUrl(serviceHostName);
            }

            var response = new OrderResponse
            {
                status = Status.ToString(),
                expires = XmlConvert.ToString(Expires, XmlDateTimeSerializationMode.Utc),
                notBefore = notBefore,
                notAfter = notAfter,
                identifiers = Identifiers.Select(x => x.GetAuthorizationIdentifierResponse()).ToList(),
                finalize = hostname,
                authorizations = Authorizations.Select(x => x.GetUrl(serviceHostName)).ToList(),
                certificate = certificateUrl
            };

            return response;
        } 
    }
}
