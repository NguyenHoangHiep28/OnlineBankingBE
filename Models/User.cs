using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class User
    {
        public User()
        {
            Accounts = new HashSet<Account>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string Avatar { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
