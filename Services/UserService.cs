using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
using OnlineBankingAPI.Models.DTOs.Requests;
using OnlineBankingAPI.Models.DTOs.Responses;
using OnlineBankingAPI.Models.Requests;
using OnlineBankingAPI.Models.Responses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        User GetById(int id);
        List<AccountDTO> GetAccounts(UserIdRequest userId);
        AccountDTO GetAccountById(AccountNumberRequest accountNumber);
        DashboardInfoResponse GetDashboardInfo(DashboardInfoRequest dashboardInfo);
        bool LockAccount(string accountNumber);
    }
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private OnlineBankingDBContext bankingContext;
        public UserService(IOptions<AppSettings> appSettings, OnlineBankingDBContext context)
        {
            _appSettings = appSettings.Value;
            bankingContext = context;
        }
        private static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = bankingContext.Users.SingleOrDefault(x => x.Phone == model.Phone);
            // return null if user not found
            if (user == null) return null;
            if (user.Active == 1)
            {
                //check authenticate attemps for security lock
                if (user.AuthAttempts < 3)
                {
                    if (user.Password != MD5Hash(model.Password))
                    {
                        user.AuthAttempts += 1;
                        string attempsAlert = "";
                        if (user.AuthAttempts < 3)
                        {
                            attempsAlert = $"{3 - user.AuthAttempts} attempts left before security account lock!";
                        }
                        else
                        {
                            attempsAlert = "Your account has been locked!";
                            user.Active = 0;
                        }
                        bankingContext.SaveChanges();
                        return new AuthenticateResponse(new List<string> { "Incorrect Password!", attempsAlert });
                    }
                }
                // authentication successful so generate jwt token
                var token = generateJwtToken(user);
                user.AuthAttempts = 0;
                bankingContext.SaveChanges();
                return new AuthenticateResponse(user, token, bankingContext);
            }
            else
            {
                return new AuthenticateResponse(new List<string> { "This account has been locked!" });
            }
        }

        public AccountDTO GetAccountById(AccountNumberRequest accountNumber)
        {
            Account account = bankingContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber.AccountNumber);
            if (account != null)
            {
                User user = bankingContext.Users.FirstOrDefault(u => u.Id == account.UserId);
                AccountDTO accountDTO = new()
                {
                    AccountNumber = account.AccountNumber,
                    Balance = account.Balance,
                    Active = account.Active,
                    CreatedAt = account.CreatedAt.ToString("dd/MM/yyyy"),
                    UserName = user.UserName
                };
                return accountDTO;
            }
            return null;

        }

        public List<AccountDTO> GetAccounts(UserIdRequest userId)
        {
            var result = GetAccountDTOs(userId.UserId);
            return result;
        }
        private List<AccountDTO> GetAccountDTOs(int userId)
        {
            var user = bankingContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user != null)
            {
                string userName = user.UserName;
                var accounts = (from accs in bankingContext.Accounts
                                where accs.UserId == userId
                                select new AccountDTO
                                {
                                    AccountNumber = accs.AccountNumber,
                                    Balance = accs.Balance,
                                    CreatedAt = accs.CreatedAt.ToString("dd/MM/yyyy"),
                                    Active = accs.Active,
                                    UserName = userName
                                }).ToList();
                return accounts;
            }
            return null;
        }
        public User GetById(int id)
        {
            return bankingContext.Users.FirstOrDefault(x => x.Id == id);
        }

        public DashboardInfoResponse GetDashboardInfo(DashboardInfoRequest dashboardInfo)
        {
            List<AccountDTO> accounts = GetAccountDTOs(dashboardInfo.UserId);
            if (accounts != null)
            {

                string thisAccountNumber = "";
                long totalBalance = 0, savingBalance = 0, thisBalance = 0;
                foreach (var account in accounts)
                {
                    if (account.AccountNumber.Equals(dashboardInfo.AccountNumber))
                    {
                        thisAccountNumber = account.AccountNumber;
                        thisBalance = account.Balance;
                    }
                    totalBalance += account.Balance;
                }
                DashboardInfoResponse response = new()
                {
                    TotalBalance = totalBalance,
                    ThisBalance = thisBalance,
                    SavingBalance = savingBalance,
                    ThisAccountNumber = dashboardInfo.AccountNumber
                };

                return response;
            }
            return null;
        }
        public bool LockAccount(string accountNumber)
        {
            bool isLocked = false;
            var account = bankingContext.Accounts.FirstOrDefault(a => a.AccountNumber.Equals(accountNumber));
            if(account != null)
            {
                if(account.Active == 1)
                {
                    account.Active = 0;
                    bankingContext.SaveChanges();
                    isLocked = true;
                }
            }
            return isLocked;
        }
        // helper methods

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 1 Hours
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
