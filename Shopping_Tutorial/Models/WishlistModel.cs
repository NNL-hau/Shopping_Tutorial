using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class WishlistModel
    {
        [Key]
        public int Id { get; set; }
        public long ProductID { get; set; }
        public string UserId { get; set; }

        [ForeignKey("ProductID")]
        public ProductModel Product { get; set; }
    }
}
