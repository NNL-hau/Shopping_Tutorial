using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class GuessPriceHistoryModel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public AppUserModel AppUser { get; set; }
        public long ProductId { get; set; }
        public decimal GuessedPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal ErrorPercent { get; set; }
        public int AwardedCoins { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
