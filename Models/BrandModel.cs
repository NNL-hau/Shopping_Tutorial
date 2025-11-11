using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập tên Thương Hiệu")]
        public String Name { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập Mô tả Thương Hiệu")]
        public String Description { get; set; }
        public String Slug { get; set; }
        public int Status { get; set; }
    }
}
