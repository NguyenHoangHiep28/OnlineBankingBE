using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class Otp
    {
        public int Id { get; set; }
        public string Otp1 { get; set; }
        public string AccountNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
