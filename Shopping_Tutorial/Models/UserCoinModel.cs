using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class UserCoinModel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public AppUserModel AppUser { get; set; }
        public int Coins { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

