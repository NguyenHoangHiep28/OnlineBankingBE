using System.ComponentModel.DataAnnotations;

namespace OnlineBankingAPI.Models.Requests
{
    public class AuthenticateRequest
    {
        [Required]
        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
