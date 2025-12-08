using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shopping_Tutorial.Repository.Validation;

namespace Shopping_Tutorial.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Slug")]
        public string Slug
        {
            get => Name.Replace(" ", "-").ToLower();
            set { }  // Đảm bảo chỉ có thể đọc Slug, không thể gán trực tiếp
        }

        [Required, MinLength(10, ErrorMessage = "Mô tả sản phẩm phải dài ít nhất 10 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        [Column(TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá vốn sản phẩm")]
        public decimal CapitalPrice { get; set; }


        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn 1 thương hiệu")]
        public int BrandID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn 1 danh mục")]
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
        public ICollection<RatingModel> Ratings { get; set; }  // ✅ Đúng
        public string Image { get; set; } = "noimage.jpg";

        // Đường dẫn hoặc URL tới file 3D (.glb/.gltf)
        [Display(Name = "Link mô hình 3D (GLB/GLTF)")]
        public string? Model3DLink { get; set; }

        // 3D Models và Annotations
        public Product3DModel? Product3D { get; set; }
        public ICollection<ProductAnnotationModel> Annotations { get; set; }
        public ICollection<ProductConfigurationModel> Configurations { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }

        [NotMapped]
        public IFormFile? Model3DUpload { get; set; }
       
    }
}
