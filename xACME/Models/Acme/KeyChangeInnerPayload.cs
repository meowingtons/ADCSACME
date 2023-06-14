using xACME.Models.DbModels;

namespace xACME.Models.Acme
{
    public class KeyChangeInnerPayload
    {
        public string account { get; set; }
        public DbAccountKey oldKey { get; set; }
    }
}