using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Responses
{
    public class SavingPackageDTO
    {
        public int Id { get; set; }
        public double Interest { get; set; }
        public string PackageName { get; set; }
        public int Duration { get; set; }
    }
}
