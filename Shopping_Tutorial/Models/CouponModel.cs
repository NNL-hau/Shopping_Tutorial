using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mã khuyển mại")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả mã khuyển mại")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số giảm thực của giảm giá ")]
        //tôi vừa thêm vào
        [Column(TypeName = "decimal(2,2)")]
        public decimal GiaNhap { get; set; } // tôi vừa thêm vào

        [Required(ErrorMessage = "Yêu cầu nhập ngày bắt đầu")]

        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập ngày kết thúc")]


        public DateTime DateExpired { get; set; }
      

        [Required(ErrorMessage = "Yêu cầu số lượng mã khuyển mại")]
        public int Quantity { get; set; }

        public int Status { get; set; }
        public string UserId { get; set; }
    }
}
