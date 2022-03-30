using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Requests
{
    public class TransferRequest
    {
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public long Amount { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
    }
}
