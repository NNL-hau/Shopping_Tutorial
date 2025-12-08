using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập tên danh mục")]
        public String Name { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập Mô tả danh mục")]
        public String Description { get; set; }
        
        public String Slug { get; set; }
        public int Status { get; set; }

    }
}
