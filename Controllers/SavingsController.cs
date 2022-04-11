using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
using OnlineBankingAPI.Models.DTOs.Requests;
using OnlineBankingAPI.Models.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Controllers
{
    [Route("controller")]
    [ApiController]
    public class SavingsController : ControllerBase
    {
        private OnlineBankingDBContext _onlineBankingDB;
        public SavingsController(OnlineBankingDBContext dBContext)
        {
            _onlineBankingDB = dBContext;
        }

        [Route("create-saving")]
        [Authorize]
        [HttpPost]
        public IActionResult CreateSaving(SavingBookRegisterRequest request)
        {
            if (request != null)
            {
                Account sourceAccount = GetAccount(request.AccountNumber);

                long trasactionFee = _onlineBankingDB.Banks.FirstOrDefault().TransactionFee;
                long souceBalance = sourceAccount.Balance - request.Amount - trasactionFee;
                SavingPackage package = GetSavingPackage(request.PackageId);
                DateTime startDate = DateTime.Now.AddDays(1).Date;
                // Get saving books count for generate new saving Id
                int savingCount = _onlineBankingDB.SavingInfos.Where(s => s.AccountNumber.Equals(sourceAccount.AccountNumber)).Count();
                string savingId = request.AccountNumber + "SB" + (savingCount + 1).ToString();
                // Add a new saving
                SavingInfo saving = new()
                {
                    AccountNumber = sourceAccount.AccountNumber,
                    Amount = request.Amount,
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(package.Duration),
                    PackageId = package.Id,
                    SavingId = savingId
                };
                _onlineBankingDB.SavingInfos.Add(saving);

                TransferCommand transferCommand = new()
                {
                    Id = "MTBT00" + (_onlineBankingDB.TransferCommands.Count() + 1).ToString(),
                    Amount = request.Amount,
                    Content = "Create saving " + "#" + savingId,
                    Type = 2,
                    FromAccountNumber = sourceAccount.AccountNumber,
                    ToAccountNumber = savingId,
                    ToCurrentBalance = request.Amount,
                    FromCurrentBalance = souceBalance
                };
                _onlineBankingDB.TransferCommands.Add(transferCommand);

                // Do transfer & add to Transaction
                Transaction createSavingTransaction = new()
                {
                    ChangedAmount = -(request.Amount),
                    AccountNumber = request.AccountNumber,
                    CreatedAt = DateTime.Now,
                    CommandId = transferCommand.Id
                };
                sourceAccount.Balance = souceBalance;
                _onlineBankingDB.Transactions.Add(createSavingTransaction);
                _onlineBankingDB.SaveChanges();
                return Ok(new { message = "Create a new saving successfully!" });
            }

            return BadRequest(new { message = "Request ERROR! Transaction Failed" });
        }
        [Route("saving-list")]
        [Authorize]
        [HttpPost]
        public IActionResult GetSavings(AccountNumberRequest accountNumberRequest)
        {
            List<SavingInfoDTO> savingInfos = new List<SavingInfoDTO>();
            string accountNumber = accountNumberRequest.AccountNumber;
            List<SavingInfo> savings = _onlineBankingDB.SavingInfos
                .Where(s => s.AccountNumber.Equals(accountNumber)).ToList();
            if(savings.Count > 0)
            {
                foreach(SavingInfo saving in savings)
                {
                    SavingPackage package = GetSavingPackage(saving.PackageId);
                    savingInfos.Add(new SavingInfoDTO
                    {
                        Id = saving.Id,
                        Amount = saving.Amount,
                        Duration = package.Duration,
                        StartDate = saving.StartDate,
                        EndDate = saving.EndDate,
                        Interest = package.Interest,
                        PackageName = package.PackageName,
                        SavingId = saving.SavingId
                    });
                }
                return Ok(savingInfos);
            }
            return NotFound(new { message = $"Not Found any savings from account {accountNumber} "});
        }
        private Account GetAccount(string accountNumber)
        {
            var account = _onlineBankingDB.Accounts.FirstOrDefault(acc => acc.AccountNumber == accountNumber);
            return account;
        }

        private SavingPackage GetSavingPackage(int pakagedId)
        {
            var pakage = _onlineBankingDB.SavingPackages.FirstOrDefault(p => p.Id == pakagedId);
            return pakage;
        }
    }
}
