using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class Account
    {
        public Account()
        {
            SavingInfos = new HashSet<SavingInfo>();
            TransferCommandFromAccountNumberNavigations = new HashSet<TransferCommand>();
            TransferCommandToAccountNumberNavigations = new HashSet<TransferCommand>();
        }

        public string AccountNumber { get; set; }
        public long? Balance { get; set; }
        public int? UserId { get; set; }
        public int? BankId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Active { get; set; }

        public virtual Bank Bank { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<SavingInfo> SavingInfos { get; set; }
        public virtual ICollection<TransferCommand> TransferCommandFromAccountNumberNavigations { get; set; }
        public virtual ICollection<TransferCommand> TransferCommandToAccountNumberNavigations { get; set; }
    }
}
