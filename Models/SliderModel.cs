using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shopping_Tutorial.Repository.Validation;

namespace Shopping_Tutorial.Models
{
    public class SliderModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên slider")]
        public String Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả")]
        public String Description { get; set; }
      
        public int Status { get; set; }
        public string Image { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }
    }
}
