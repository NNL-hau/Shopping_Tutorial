using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shopping_Tutorial.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }
        public List<RatingModel> Ratings { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập bình luận sản phẩm")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        public string Email { get; set; }
    }
}
