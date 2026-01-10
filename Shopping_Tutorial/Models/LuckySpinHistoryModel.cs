using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class LuckySpinHistoryModel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public AppUserModel AppUser { get; set; }
        public string PrizeType { get; set; }
        public decimal PrizeValue { get; set; }
        public string CouponCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
