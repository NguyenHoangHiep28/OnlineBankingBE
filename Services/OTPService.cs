using Microsoft.AspNetCore.Mvc;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace OnlineBankingAPI.Services
{
    public interface IOTPService
    {
        MessageResource.StatusEnum SendOTP(string sOTP, string phoneNumber);
        string GenerateRandomOTP(string accountNumber);
        IActionResult VerifyOTP(OTPVerificationRequest oTPVerification);
        IActionResult GetOTP(OTPSendRequest request);
    }
    public class OTPService : ControllerBase,IOTPService
    {
        private OnlineBankingDBContext bankingContext;
        public OTPService(OnlineBankingDBContext onlineBankingDB)
        {
            bankingContext = onlineBankingDB;
        }
        public IActionResult VerifyOTP(OTPVerificationRequest oTPVerification)
        {
            var myOTP = bankingContext.Otps.FirstOrDefault(o => o.AccountNumber == oTPVerification.AccountNumber);
            if (myOTP != null)
            {
                if (myOTP.Otp1 == oTPVerification.OTP)
                {
                    var comparator = DateTime.Compare(DateTime.Now, myOTP.ExpiredAt);
                    if (comparator >= 0)
                    {
                        return StatusCode(410, "This OTP has been expired! Please request a new OTP");
                    }

                    bankingContext.Otps.Remove(myOTP);
                    bankingContext.SaveChanges();
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
        public string GenerateRandomOTP(string accountNumber)

        {

            string sOTP = String.Empty;
            char[] saAllowedCharacters = accountNumber.ToCharArray();
            Random rand = new Random();

            for (int i = 0; i < 6; i++)
            {

                int p = rand.Next(0, saAllowedCharacters.Length);

                char sTempChars = saAllowedCharacters[p];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        public MessageResource.StatusEnum SendOTP(string sOTP, string phoneNumber)
        {
            string accountSid = "ACc619004490dbd4138e1b385e9b256972";
            string authToken = "607cf2d95b9627bcbeb97347dee264a6";
            //3c736c9a89fc77017babb9a757db5a55
            string verificationPhone;
            string vietnamCode = "+84";
            phoneNumber.Remove(0);
            // Format phone number to +84xxxxxxxxx
            verificationPhone = vietnamCode + phoneNumber;
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "MTBANK : Your transfer verification OTP is : " + sOTP,
                from: new Twilio.Types.PhoneNumber("+16099973225"),
                to: new Twilio.Types.PhoneNumber(verificationPhone)
            );

            return message.Status;
        }
        public IActionResult GetOTP(OTPSendRequest request)
        {
            // generate OTP
            string myOTP = GenerateRandomOTP(request.AccountNumber);

            // Check if there existing an OTP of account then remove
            var OTP = bankingContext.Otps.FirstOrDefault(o => o.AccountNumber == request.AccountNumber);
            if (OTP != null)
            {
                bankingContext.Remove(OTP);
                bankingContext.SaveChanges();
            }
            // Create & save a new OTP
            var time = DateTime.Now;
            var otp = new Otp()
            {
                Otp1 = myOTP,
                AccountNumber = request.AccountNumber,
                CreatedAt = time,
                ExpiredAt = time.AddMinutes(5)
            };
            bankingContext.Otps.Add(otp);
            bankingContext.SaveChanges();
            // Send OTP to Phone
            var success = SendOTP(myOTP, request.PhoneNumber);
            if (success.ToString().Equals("queued"))
            {

                return Ok("OTP has been send to your phone!");
            }
            else
            {
                return BadRequest("Cannot send OTP ...");
            }

        }
    }
}
