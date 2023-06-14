using System;
using System.Collections.Generic;
using System.Net;

namespace xACME.Models.DbModels
{
    public class DbAccount
    {
        public Guid Id { get; set; }
        public DbAccountKey Key { get; set; }
        public List<string> Contact { get; set; }
        public IPAddress InitialIp { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
