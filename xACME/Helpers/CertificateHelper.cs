using System;
using Microsoft.Extensions.Configuration;

namespace xACME.Helpers
{
    public static class CertificateHelper
    {
        public static string GetPemChainResponse(IConfiguration configuration, int requestId)
        {
            var adcs = new Adcs(configuration["AdcsConfig:HostName"], configuration["AdcsConfig:Name"]);

            var cert = adcs.RetrieveCertificate(requestId);
            return cert + configuration["AdcsConfig:PemCertChain"];
        }
    }
}
