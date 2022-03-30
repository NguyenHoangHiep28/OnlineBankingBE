using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class SavingInfo
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SavingId { get; set; }
        public string AccountNumber { get; set; }
        public int PackageId { get; set; }

        public virtual Account AccountNumberNavigation { get; set; }
        public virtual SavingPackage Package { get; set; }
    }
}
