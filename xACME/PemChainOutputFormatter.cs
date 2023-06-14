using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace xACME
{
    public class PemChainOutputFormatter : TextOutputFormatter
    {
        public PemChainOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/pem-certificate-chain"));

            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(string).IsAssignableFrom(type)
                || typeof(IEnumerable<string>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }

            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();
            buffer.AppendLine(context.Object.ToString());

            return response.WriteAsync(buffer.ToString());
        }
    }
}
