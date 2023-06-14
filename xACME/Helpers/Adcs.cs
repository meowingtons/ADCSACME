using System;
using CERTCLILib;

namespace xACME.Helpers
{
    public class Adcs
    {
        private CertificationAuthority _ca { get; set; }
        private const int CC_DEFAULTCONFIG = 0;
        private const int CC_UIPICKCONFIG = 0x1;
        private const int CR_IN_BASE64 = 0x1;
        private const int CR_IN_FORMATANY = 0;
        private const int CR_IN_PKCS10 = 0x100;
        private const int CR_DISP_ISSUED = 0x3;
        private const int CR_DISP_UNDER_SUBMISSION = 0x5;
        private const int CR_OUT_BASE64 = 0x1;
        private const int CR_OUT_CHAIN = 0x100;
        private const int CR_IN_ENCODEANY = 0xff;
        private const int CR_OUT_BASE64HEADER = 0x0;

        public Adcs(CertificationAuthority ca)
        {
            _ca = ca;
        }

        public Adcs(string hostName, string caName) : this(new CertificationAuthority(hostName, caName))
        {
        }

        public class CertificationAuthority
        {
            public string HostName { get; set; }
            public string CaName { get; set; }

            public CertificationAuthority(string hostName, string caName)
            {
                HostName = hostName;
                CaName = caName;
            }

            public string ConnectionString => HostName + "\\" + CaName;
        }

        public class CertificateRequest
        {
            public string Csr { get; set; }
            public string Attributes { get; set; }

            public CertificateRequest(string csr, string attributes = null)
            {
                Csr = csr;
                Attributes = attributes;
            }
        }

        public int SendCertificateRequest(CertificateRequest certRequest)
        {
            var objCertRequest = new CCertRequest();

            var iDisposition = objCertRequest.Submit(
                    CR_IN_ENCODEANY | CR_IN_FORMATANY,
                    certRequest.Csr,
                    certRequest.Attributes,
                    _ca.ConnectionString);

            switch (iDisposition)
            {
                case CR_DISP_ISSUED:
                    return objCertRequest.GetRequestId();
                case CR_DISP_UNDER_SUBMISSION:
                    throw new Exception("The certificate is still pending.");
                default:
                    throw new Exception("The submission failed: " + objCertRequest.GetDispositionMessage());
            }
        }

        public string RetrieveCertificate(int requestId)
        {
            var objCertRequest = new CCertRequest();
            var iDisposition = objCertRequest.RetrievePending(requestId, _ca.ConnectionString);

            if (iDisposition != CR_DISP_ISSUED)
            {
                throw new Exception("Certificate was not issued.");
            }

            var cert = objCertRequest.GetCertificate(CR_OUT_BASE64HEADER);
            return cert;
        }
    }
}
