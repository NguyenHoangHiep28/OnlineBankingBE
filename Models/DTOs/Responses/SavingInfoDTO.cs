using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Responses
{
    public class SavingInfoDTO
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SavingId { get; set; }
        public long Amount { get; set; }
        public double Interest { get; set; }
        public string PackageName { get; set; }
        public int Duration { get; set; }
    }
}
