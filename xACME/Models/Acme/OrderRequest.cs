using System.Collections.Generic;

namespace xACME.Models.Acme
{
    public class OrderRequest
    {
        public List<AuthorizationIdentifier> Identifiers { get; set; }
        //public string NotBefore { get; set; }
        //public string NotAfter { get; set; }
        //public DateTime ParsedNotBefore => XmlConvert.ToDateTime(NotBefore, XmlDateTimeSerializationMode.Utc);
        //public DateTime ParsedNotAfter => XmlConvert.ToDateTime(NotAfter, XmlDateTimeSerializationMode.Utc);

        //public bool IsValidityPeriodAcceptable(int days)
        //{
        //    if (NotBefore != null && NotAfter != null)
        //    {
        //        return (ParsedNotBefore - ParsedNotAfter).Days == days;
        //    }

        //    return true;
        //}
    }
}