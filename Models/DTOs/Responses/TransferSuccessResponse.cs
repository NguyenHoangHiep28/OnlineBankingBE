using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.DTOs.Responses
{
    public class TransferSuccessResponse
    {
        public string TransactionId { get; set; }
        public DateTime TransferTime { get; set; }
    }
}
