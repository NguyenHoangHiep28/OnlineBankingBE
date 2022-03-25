using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs
{
    public class AccountDTO
    {
        public string AccountNumber { get; set; }
        public long Balance { get; set; }
        public int Active { get; set; }
        public string CreatedAt { get; set; }
    }
}
