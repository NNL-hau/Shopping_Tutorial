using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class UserCartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } // User Id từ Identity
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        // Thông tin cấu hình tùy chỉnh (nếu có)
        public int? ConfigurationId { get; set; }
        
        [ForeignKey("ConfigurationId")]
        public ProductConfigurationModel? Configuration { get; set; }

        // Lưu thông tin cấu hình dạng JSON để dễ hiển thị
        public string? ConfigurationData { get; set; } // JSON: {"color": "#3498db", "material": "metal", "components": [...]}

        public decimal Total => Quantity * Price;
    }

}
