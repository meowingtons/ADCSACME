using System.Collections.Generic;

namespace xACME.Models.Acme
{
    public class MetaDirectoryObject
    {
        public string termsOfService { get; set; }
        public string website { get; set; }
        public List<string> caaIdentities { get; set; }
        public bool externalAccountRequired { get; set; }
    }
}