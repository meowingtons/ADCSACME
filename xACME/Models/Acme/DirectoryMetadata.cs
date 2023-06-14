namespace xACME.Models.Acme
{
    public class DirectoryMetadata
    {
        public string newNonce { get; set; }
        public string newAccount { get; set; }
        public string newOrder { get; set; }
        public string newAuthz { get; set; }
        public string revokeCert { get; set; }
        public string keyChange { get; set; }
        public MetaDirectoryObject meta { get; set; }
    }
}