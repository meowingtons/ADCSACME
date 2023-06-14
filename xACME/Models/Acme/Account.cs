using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace xACME.Models.Acme
{
    public class Account
    {
        public string status { get; set; }
        public List<string> contact { get; set; }
        public object externalAccountBinding { get; set; }
        public bool termsOfServiceAgreed { get; set; }
        public string orders { get; set; }
        public bool onlyReturnExisting { get; set; }
    }
}
