using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }
        public long? ChangedAmount { get; set; }
        public string AccountNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CommandId { get; set; }

        public virtual TransferCommand Command { get; set; }
    }
}
