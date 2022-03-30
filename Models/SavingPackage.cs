using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class SavingPackage
    {
        public SavingPackage()
        {
            SavingInfos = new HashSet<SavingInfo>();
        }

        public int Id { get; set; }
        public double Interest { get; set; }
        public string PackageName { get; set; }
        public int Duration { get; set; }

        public virtual ICollection<SavingInfo> SavingInfos { get; set; }
    }
}
