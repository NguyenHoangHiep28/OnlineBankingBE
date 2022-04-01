using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Responses
{
    public class TransactionHistory
    {
        public string Id { get; set; }
        public string MyAccountNumber { get; set; }
        public string PartnerAccountNumber { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public long MyCurrentBalance { get; set; }
        public long ChangedAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
