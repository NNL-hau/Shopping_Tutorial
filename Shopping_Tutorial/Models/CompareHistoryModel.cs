using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class CompareHistoryModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public AppUserModel AppUser { get; set; }

        public string ComparedProductNames { get; set; } // Storing product IDs as a comma-separated string

        public DateTime ComparisonDate { get; set; } = DateTime.Now;
    }
}
