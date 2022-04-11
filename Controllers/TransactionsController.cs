using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
using OnlineBankingAPI.Models.DTOs.Requests;
using OnlineBankingAPI.Models.DTOs.Responses;
using OnlineBankingAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace OnlineBankingAPI.Controllers
{
    [ApiController]
    [Route("controller")]
    public class TransactionsController : ControllerBase
    {
        private OnlineBankingDBContext _onlineBankingDB;
        private IOTPService _OTPService;
        public TransactionsController(OnlineBankingDBContext dBContext, IOTPService OTPService)
        {
            _onlineBankingDB = dBContext;
            _OTPService = OTPService;
        }
        [Route("transfer")]
        [Authorize]
        [HttpPost]
        public IActionResult Transfer(TransferRequest transferRequest)
        {
            if (transferRequest != null)
            {
                // Add transfercommand to DB
                Account fromAccount = GetAccount(transferRequest.FromAccountNumber);
                Account toAccount = GetAccount(transferRequest.ToAccountNumber);

                long trasactionFee = _onlineBankingDB.Banks.FirstOrDefault().TransactionFee;
                long fromBalance = fromAccount.Balance - transferRequest.Amount - trasactionFee;
                long toBalance = toAccount.Balance + transferRequest.Amount;

                TransferCommand transferCommand = new()
                {
                    Id = "MTBT00" + (_onlineBankingDB.TransferCommands.Count() + 1).ToString(),
                    Amount = transferRequest.Amount,
                    Content = transferRequest.Content,
                    Type = transferRequest.Type,
                    FromAccountNumber = fromAccount.AccountNumber,
                    ToAccountNumber = toAccount.AccountNumber,
                    ToCurrentBalance = toBalance,
                    FromCurrentBalance = fromBalance

                };
                _onlineBankingDB.TransferCommands.Add(transferCommand);
                _onlineBankingDB.SaveChanges();
                // Do transfer & add to Transaction
                Transaction fromTransaction = new()
                {
                    ChangedAmount = -(transferRequest.Amount),
                    AccountNumber = transferRequest.FromAccountNumber,
                    CreatedAt = DateTime.Now,
                    CommandId = transferCommand.Id
                };

                fromAccount.Balance = fromBalance;
                _onlineBankingDB.Transactions.Add(fromTransaction);

                Transaction toTransaction = new()
                {
                    ChangedAmount = transferRequest.Amount,
                    AccountNumber = transferRequest.ToAccountNumber,
                    CreatedAt = DateTime.Now,
                    CommandId = transferCommand.Id
                };
                toAccount.Balance = toBalance;
                _onlineBankingDB.Transactions.Add(toTransaction);

                _onlineBankingDB.SaveChanges();

                // Send notification message to receiver phone
                User receiver = GetUser(toAccount);
                _OTPService.SendReceivedTransferMessageNotification
                    (   
                        toTransaction.CreatedAt,
                        receiver.Phone,
                        toAccount.AccountNumber,
                        fromAccount.AccountNumber,
                        transferRequest.Amount,
                        toBalance,
                        transferCommand.Content
                    );

                return Ok(new TransferSuccessResponse()
                {
                    TransactionId = transferCommand.Id,
                    TransferTime = fromTransaction.CreatedAt
                });
            }

            return BadRequest("Request ERROR! Transaction Failed");
        }

        [Route("transaction-otp")]
        [HttpPost]
        public IActionResult GetOTP(OTPSendRequest request)
        {
            var result = _OTPService.GetOTP(request);
            return result;
        }
        [Route("totp-verify")]
        [HttpPost]
        public IActionResult VerifyOTP(OTPVerificationRequest oTPVerification)
        {
            var result = _OTPService.VerifyOTP(oTPVerification);
            return result;
        }

        [Route("transaction-history")]
        [Authorize]
        [HttpPost]
        public IActionResult GetTransactionHistory(AccountNumberRequest accountNumber)
        {
            List<TransferCommand> transferCommands = _onlineBankingDB.TransferCommands.Where
                (
                    t => t.FromAccountNumber.Equals(accountNumber.AccountNumber) ||
                    t.ToAccountNumber.Equals(accountNumber.AccountNumber)
                ).Include(t => t.Transactions).ToList();
            if (transferCommands.Count > 0)
            {
                List<TransactionHistory> transactionHistories = new List<TransactionHistory>();
                long myCurrentBalance = 0;
                string myAccountNumber = "", partnerAccountNumber = "", content = "", partnerName = "";
                long changeAmount = 0;
                DateTime createdAt;
                TransactionHistory history;
                foreach (var tCommand in transferCommands)
                {
                    changeAmount = tCommand.Transactions.FirstOrDefault(t => t.AccountNumber.Equals(accountNumber.AccountNumber)).ChangedAmount;
                    if (tCommand.FromAccountNumber.Equals(accountNumber.AccountNumber))
                    {
                        myAccountNumber = tCommand.FromAccountNumber;
                        myCurrentBalance = tCommand.FromCurrentBalance;
                        partnerAccountNumber = tCommand.ToAccountNumber;
                    }
                    else
                    {
                        myAccountNumber = tCommand.ToAccountNumber;
                        myCurrentBalance = tCommand.ToCurrentBalance;
                        partnerAccountNumber = tCommand.FromAccountNumber;
                    }
                    content = tCommand.Content;
                    createdAt = tCommand.Transactions.FirstOrDefault(t => t.AccountNumber.Equals(accountNumber.AccountNumber)).CreatedAt;
                    if (tCommand.Type == 1)
                    {
                        partnerName = _onlineBankingDB.Users.FirstOrDefault(u => u.Id == GetAccount(partnerAccountNumber).UserId).UserName;
                    }
                    else if (tCommand.Type == 2)
                    {
                        partnerName = GetUser(GetAccount(accountNumber.AccountNumber)).UserName;
                    }
                    history = new()
                    {
                        Id = tCommand.Id,
                        ChangedAmount = changeAmount,
                        Content = content,
                        CreatedAt = createdAt,
                        MyAccountNumber = myAccountNumber,
                        MyCurrentBalance = myCurrentBalance,
                        PartnerAccountNumber = partnerAccountNumber,
                        Type = tCommand.Type,
                        PartnerName = partnerName
                    };
                    transactionHistories.Add(history);
                }
                List<TransactionHistory> sortedHistories = transactionHistories.OrderByDescending(t => t.CreatedAt).ToList();
                return Ok(sortedHistories);
            }
            return BadRequest("Your account has no transactions yet");
        }
        private Account GetAccount(string accountNumber)
        {
            var account = _onlineBankingDB.Accounts.FirstOrDefault(acc => acc.AccountNumber == accountNumber);
            return account;
        }

        private User GetUser(Account account)
        {
            var user = _onlineBankingDB.Users.FirstOrDefault(u => u.Id == account.UserId);
            return user;
        }

    }

}
