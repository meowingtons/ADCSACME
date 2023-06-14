namespace xACME.Models.Acme
{
    public class ChallengeResponse
    {
        public string type { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public string validated { get; set; }
        public Error error { get; set; }
        public string token { get; set; }
    }
}