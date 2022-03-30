using OnlineBankingAPI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Models.Responses
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<AccountDTO> Accounts { get; set; }
        public string Token { get; set; }

        public List<string> Errors { get; set; }

        public AuthenticateResponse(User user, string token, OnlineBankingDBContext db)
        {
            Id = user.Id;
            UserName = user.UserName;
            Phone = user.Phone;
            Email = user.Email;
            Accounts = (from accs in db.Accounts
                       where accs.UserId == user.Id
                       select new AccountDTO 
                       {
                           AccountNumber = accs.AccountNumber,
                           Balance = accs.Balance,
                           CreatedAt = accs.CreatedAt.ToString("MM/dd/yyyy")
                       }).ToList();
            Token = token;
        }

        public AuthenticateResponse(List<string> errors)
        {
            Errors = errors;
        }
    }
}
