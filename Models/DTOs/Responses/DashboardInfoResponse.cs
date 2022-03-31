using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Responses
{
    public class DashboardInfoResponse
    {
        public long TotalBalance { get; set; }
        public long ThisBalance { get; set; }
        public string ThisAccountNumber { get; set; }
        public long SavingBalance { get; set; }
    }
}
