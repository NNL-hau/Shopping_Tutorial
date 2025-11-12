using System.Text.Json.Serialization;

namespace Shopping_Tutorial.Models
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MeshName { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}

