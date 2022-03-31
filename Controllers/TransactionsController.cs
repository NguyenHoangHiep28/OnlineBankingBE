using Microsoft.AspNetCore.Mvc;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
using OnlineBankingAPI.Models.DTOs.Requests;
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
        public TransactionsController(OnlineBankingDBContext dBContext)
        {
            _onlineBankingDB = dBContext;
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

                return Ok("Transfer successfully!");
            }

            return BadRequest("Request ERROR! Transaction Failed");
        }

        [Route("transaction-otp")]
        [Authorize]
        [HttpPost]
        public IActionResult GetOTP(AccountNumberRequest accountNumber /*, string phoneNumber */)
        {
            // generate OTP
            string myOTP = GenerateRandomOTP(accountNumber);
            var account = GetAccount(accountNumber.AccountNumber);
            if (account != null)
            {
                // Check if there existing an OTP of account then remove
                var OTP = _onlineBankingDB.Otps.FirstOrDefault(o => o.AccountNumber == account.AccountNumber);
                if (OTP != null)
                {
                    _onlineBankingDB.Remove(OTP);
                    _onlineBankingDB.SaveChanges();
                }
                // Create & save a new OTP
                var time = DateTime.Now;
                var otp = new Otp()
                {
                    Otp1 = myOTP,
                    AccountNumber = account.AccountNumber,
                    CreatedAt = time,
                    ExpiredAt = time.AddMinutes(5)
                };
                _onlineBankingDB.Otps.Add(otp);
                _onlineBankingDB.SaveChanges();
                // Send OTP to Phone
                var success = SendOTP(myOTP);
                if (success.ToString().Equals("queued"))
                {

                    return Ok("OTP has been send to your phone!");
                }
                else
                {
                    return BadRequest("Cannot send OTP ...");
                }
            }
            else
            {
                return NotFound("Not Found Account Number for executing transfer");
            }
        }
        [Route("totp-verify")]
        [Authorize]
        [HttpPost]
        public IActionResult VerifyOTP(OTPVerificationRequest oTPVerification)
        {
            var myOTP = _onlineBankingDB.Otps.FirstOrDefault(o => o.AccountNumber == oTPVerification.AccountNumber);
            if (myOTP != null)
            {
                if (myOTP.Otp1 == oTPVerification.OTP)
                {
                    var comparator = DateTime.Compare(DateTime.Now, myOTP.ExpiredAt);
                    if (comparator >= 0)
                    {
                        return StatusCode(410, "This OTP has been expired! Please request a new OTP");
                    }

                    _onlineBankingDB.Otps.Remove(myOTP);
                    _onlineBankingDB.SaveChanges();
                    return Ok();
                }
                else
                {
                    return BadRequest("OTP isn't match! Please try again.");
                }
            }
            else
            {
                return BadRequest("OTP isn't match! Please try again.");
            }
        }

        private string GenerateRandomOTP(AccountNumberRequest accountNumber)

        {

            string sOTP = String.Empty;
            char[] saAllowedCharacters = accountNumber.AccountNumber.ToCharArray();
            Random rand = new Random();

            for (int i = 0; i < 6; i++)
            {

                int p = rand.Next(0, saAllowedCharacters.Length);

                char sTempChars = saAllowedCharacters[p];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        private MessageResource.StatusEnum SendOTP(string sOTP /*, string phoneNumber */)
        {
            string accountSid = "ACc619004490dbd4138e1b385e9b256972";
            string authToken = "8b28e8cb9354a1f235031bc26622452d";
            string myVerificationPhone = "+84348483145";
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "MTBANK : Your transfer verification is : " + sOTP,
                from: new Twilio.Types.PhoneNumber("+16099973225"),
                to: new Twilio.Types.PhoneNumber(myVerificationPhone)
            );

            return message.Status;
        }

        private Account GetAccount(string accountNumber)
        {
            var account = _onlineBankingDB.Accounts.FirstOrDefault(acc => acc.AccountNumber == accountNumber);
            return account;
        }

    }
}
