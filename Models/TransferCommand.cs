using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class TransferCommand
    {
        public TransferCommand()
        {
            Transactions = new HashSet<Transaction>();
        }

        public string Id { get; set; }
        public long Amount { get; set; }
        public int Type { get; set; }
        public long ToCurrentBalance { get; set; }
        public string FromAccountNumber { get; set; }
        public long FromCurrentBalance { get; set; }
        public string ToAccountNumber { get; set; }
        public string Content { get; set; }

        public virtual Account FromAccountNumberNavigation { get; set; }
        public virtual Account ToAccountNumberNavigation { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
