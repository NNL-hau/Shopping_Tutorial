using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class SearchHistoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string SearchTerm { get; set; }

        public DateTime SearchDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public AppUserModel User { get; set; }
    }
}
