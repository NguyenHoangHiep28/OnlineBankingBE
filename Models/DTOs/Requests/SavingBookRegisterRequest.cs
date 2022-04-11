using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Requests
{
    public class SavingBookRegisterRequest
    {
        public string AccountNumber { get; set; }
        public int PackageId { get; set; }
        public long Amount { get; set; }
    }
}
