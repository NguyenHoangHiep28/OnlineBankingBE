using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
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
        List<AccountDTO> GetAccounts(int userId);
    }
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private OnlineBankingContext bankingContext;
        public UserService(IOptions<AppSettings> appSettings, OnlineBankingContext context)
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
            if(user.Active == 1)
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

        public List<AccountDTO> GetAccounts(int userId)
        {
            var accounts = (from accs in bankingContext.Accounts
                           where accs.UserId == userId
                           select new AccountDTO
                           {
                               AccountNumber = accs.AccountNumber,
                               Balance = accs.Balance,
                               CreatedAt = accs.CreatedAt,
                               Active = (int)accs.Active
                           }).ToList();
            if (accounts!= null)
            {
                return accounts;
            }
            return null;
        }

        public User GetById(int id)
        {
            return bankingContext.Users.FirstOrDefault(x => x.Id == id);
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
