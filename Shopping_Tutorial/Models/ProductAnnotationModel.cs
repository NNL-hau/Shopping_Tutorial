using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class ProductAnnotationModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long ProductID { get; set; }

        [ForeignKey("ProductID")]
        public ProductModel Product { get; set; }

        // Tiêu đề annotation
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        // Nội dung annotation
        [Required]
        public string Content { get; set; }

        // Vị trí 3D của annotation (X, Y, Z)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PositionX { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PositionY { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PositionZ { get; set; }

        // Màu của marker (hex color)
        public string? MarkerColor { get; set; } = "#ff4444";

        // Thứ tự hiển thị
        public int DisplayOrder { get; set; } = 0;

        // Có hiển thị mặc định không
        public bool IsDefault { get; set; } = false;

        // Người tạo (nếu là user tạo)
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}

