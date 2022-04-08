using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Requests
{
    public class OTPSendRequest
    {
        public string AccountNumber { get; set; }
        public string PhoneNumber  { get; set; }
    }
}
