using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class Bank
    {
        public Bank()
        {
            Accounts = new HashSet<Account>();
        }

        public int Id { get; set; }
        public string BankName { get; set; }
        public string Logo { get; set; }
        public string BankAddress { get; set; }
        public long? TransactionFee { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
