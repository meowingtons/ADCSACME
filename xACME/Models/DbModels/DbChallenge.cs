using System;
using xACME.Models.Acme;

namespace xACME.Models.DbModels
{
    public class DbChallenge
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public ChallengeStatus Status { get; set; }
        public DateTime Validated { get; set; }
        public string Error { get; set; }
        public string Token { get; set; }

        public ChallengeResponse GetChallengeResponse(string serviceHostName) => new ChallengeResponse
        {
            type = Type,
            token = Token,
            url = "https://" + serviceHostName + "/acme/chall/" + Id,
            status = Status.ToString()
        };
    }
}
