using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Requests
{
    public class DashboardInfoRequest
    {
        public int UserId { get; set; }
        public string AccountNumber { get; set; }
    }
}
