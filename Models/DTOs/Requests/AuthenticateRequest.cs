using System.ComponentModel.DataAnnotations;

namespace OnlineBankingAPI.Models.Requests
{
    public class AuthenticateRequest
    {
        public string Phone { get; set; }
        public string Password { get; set; }

    }
}
