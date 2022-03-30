using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Requests
{
    public class OTPVerificationRequest
    {
        public string AccountNumber { get; set; }
        public string OTP { get; set; }
    }
}
