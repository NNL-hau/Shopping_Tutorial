using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class ShippingModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu chọn tên xã phường")]
        public string Ward { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn quận huyện")]
        public string District { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn thành phố")]
        public string City { get; set; }


    }
}
