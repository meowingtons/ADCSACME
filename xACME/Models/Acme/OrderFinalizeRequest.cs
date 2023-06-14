using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509CertificateRequests;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Jose;
using SysadminsLV.Asn1Parser;
using SysadminsLV.PKI.Cryptography.X509CertificateRequests;

namespace xACME.Models.Acme
{
    public class OrderFinalizeRequest
    {
        private X509CertificateRequestPkcs10 _request;
        private List<string> _identifiers;

        public string csr { get; set; }
        public byte[] CsrDecoded => Base64Url.Decode(csr);
        public X509CertificateRequestPkcs10 RequestPkcs10 => _request ?? (_request = GetPkcs10Request());
        public List<string> Identifiers => _identifiers ?? (_identifiers = GetUniqueIdentifiers());
        public string Cn => Regex.Replace(GetPkcs10Request().SubjectName.Name?.Split(',').FirstOrDefault(x => x.StartsWith("cn=", StringComparison.CurrentCultureIgnoreCase)) ?? throw new InvalidOperationException(), "cn=", "", RegexOptions.IgnoreCase);
        public string CsrDecodedString => AsnFormatter.BinaryToString(CsrDecoded, EncodingType.Base64RequestHeader,
            EncodingFormat.CRLF, 0, 0, true);

        private X509CertificateRequest GetPkcs10Request()
        {
            var request = new X509CertificateRequest(CsrDecoded);

            return request;
        }

        private List<string> GetUniqueIdentifiers()
        {
            var result = new List<string> {Cn};

            var pkcs10 = GetPkcs10Request();

            foreach (var extension in pkcs10.Extensions)
            {
                if (extension.Oid.FriendlyName != "Subject Alternative Name") continue;

                var identifier = ((X509SubjectAlternativeNamesExtension) extension).AlternativeNames.AsEnumerable().Select(x => x.Value);

                result.AddRange(identifier.Except(result));
            }
         
            return result;
        }
    }
}