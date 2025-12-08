using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class ProductConfigurationModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long ProductID { get; set; }

        [ForeignKey("ProductID")]
        public ProductModel Product { get; set; }

        // User ID nếu là cấu hình của user
        public string? UserId { get; set; }

        // Màu sắc đã chọn
        [StringLength(50)]
        public string? SelectedColor { get; set; }

        // Vật liệu đã chọn
        [StringLength(50)]
        public string? SelectedMaterial { get; set; }

        // Linh kiện đã chọn (JSON string để lưu danh sách)
        public string? SelectedComponents { get; set; } // Format: JSON array ["screen", "keyboard", "battery"]

        // Giá sau khi tùy chỉnh (nếu có thay đổi giá)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomPrice { get; set; }

        // Tên cấu hình (nếu user đặt tên)
        [StringLength(200)]
        public string? ConfigurationName { get; set; }

        // Có phải cấu hình mặc định không
        public bool IsDefault { get; set; } = false;

        // Có được lưu vào giỏ hàng không
        public bool IsInCart { get; set; } = false;

        // ID của cart item nếu đã thêm vào giỏ
        public int? CartItemId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}

