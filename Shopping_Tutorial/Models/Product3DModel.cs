using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class Product3DModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long ProductID { get; set; }

        [ForeignKey("ProductID")]
        public ProductModel Product { get; set; }

        // Đường dẫn file 3D model (GLB, GLTF, OBJ, etc.)
        public string? Model3DPath { get; set; }

        // Đường dẫn texture nếu có
        public string? TexturePath { get; set; }

        // Kích thước mặc định của model (để scale)
        public decimal? DefaultScale { get; set; } = 1.0m;

        // Vị trí mặc định của camera
        public decimal? CameraPositionX { get; set; } = 0;
        public decimal? CameraPositionY { get; set; } = 2;
        public decimal? CameraPositionZ { get; set; } = 5;

        // Có hỗ trợ AR không
        public bool SupportAR { get; set; } = true;

        // Có hỗ trợ VR không
        public bool SupportVR { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}

