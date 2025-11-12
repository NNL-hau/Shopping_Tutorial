using System.Collections.Generic;
using System.Linq;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Products.Any())
            {
                return;
            }

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Falcon X1 Ultrabook",
                    Slug = "falcon-x1-ultrabook",
                    Description = "Ultrabook mỏng nhẹ cấu hình cao, phù hợp cho công việc sáng tạo.",
                    ModelUrl = "/models/falcon-x1.glb",
                    ThumbnailUrl = "/images/falcon-x1-thumb.jpg",
                    Weight = 1.35,
                    Material = "Hợp kim nhôm anodized",
                    Category = "Laptop",
                    Manufacturer = "Falcon Labs",
                    Components = new List<Component>
                    {
                        new Component
                        {
                            Name = "Vỏ trên",
                            MeshName = "TopCover",
                            Material = "Nhôm anodized",
                            Description = "Vỏ nhôm nguyên khối bảo vệ phần màn hình và bản lề.",
                            Weight = 0.48,
                            Notes = "Có logo Falcon khắc laser."
                        },
                        new Component
                        {
                            Name = "Module màn hình 14\"",
                            MeshName = "ScreenAssembly",
                            Material = "Kính Gorilla Glass 5",
                            Description = "Tấm nền 2.8K OLED hỗ trợ 100% DCI-P3.",
                            Weight = 0.38,
                            Notes = "Độ sáng tối đa 600 nits."
                        },
                        new Component
                        {
                            Name = "Bàn phím cơ chế cắt kéo",
                            MeshName = "KeyboardDeck",
                            Material = "Nhựa ABS phủ UV",
                            Description = "Hành trình 1.4mm, đèn nền RGB ba vùng.",
                            Weight = 0.21,
                            Notes = "Khung gia cố bằng sợi carbon."
                        },
                        new Component
                        {
                            Name = "Hệ thống tản nhiệt Vapor",
                            MeshName = "CoolingChamber",
                            Material = "Buồng hơi đồng",
                            Description = "Buồng hơi kép với hai quạt 90mm, hiệu quả cao.",
                            Weight = 0.16,
                            Notes = "Có cảm biến nhiệt độ thời gian thực."
                        },
                        new Component
                        {
                            Name = "Pin 75Wh",
                            MeshName = "BatteryPack",
                            Material = "Lithium Polymer",
                            Description = "Pin 4 cell dung lượng cao cho thời lượng lên đến 12h.",
                            Weight = 0.12,
                            Notes = "Hỗ trợ sạc nhanh 100W USB-C."
                        }
                    }
                },
                new Product
                {
                    Name = "Raptor Z5 Gaming Laptop",
                    Slug = "raptor-z5-gaming-laptop",
                    Description = "Laptop gaming hiệu năng cao, dành cho đồ họa và eSports.",
                    ModelUrl = "/models/raptor-z5.glb",
                    ThumbnailUrl = "/images/raptor-z5-thumb.jpg",
                    Weight = 2.45,
                    Material = "Hợp kim magie",
                    Category = "Laptop",
                    Manufacturer = "Raptor Studio",
                    Components = new List<Component>
                    {
                        new Component
                        {
                            Name = "Nắp lưng RGB",
                            MeshName = "LidRGB",
                            Material = "Nhôm CNC",
                            Description = "Nắp lưng với hiệu ứng RGB tuỳ biến.",
                            Weight = 0.62,
                            Notes = "Hỗ trợ đồng bộ RaptorSync."
                        },
                        new Component
                        {
                            Name = "Module màn hình 16\" QHD",
                            MeshName = "Display16QHD",
                            Material = "Kính chống chói",
                            Description = "Màn hình 240Hz với chuẩn HDR600.",
                            Weight = 0.44,
                            Notes = "Có cảm biến ánh sáng môi trường."
                        },
                        new Component
                        {
                            Name = "Bàn phím cơ quang học",
                            MeshName = "OpticalKeyboard",
                            Material = "Nhựa PBT double-shot",
                            Description = "Switch quang học hành trình 1.8mm.",
                            Weight = 0.32,
                            Notes = "Có numpad ảo cảm ứng."
                        },
                        new Component
                        {
                            Name = "Hệ thống tản nhiệt HydraFlow",
                            MeshName = "HydraFlowCooling",
                            Material = "Đồng + Graphite",
                            Description = "4 ống đồng, 3 quạt, 2 buồng hơi kết hợp graphite.",
                            Weight = 0.26,
                            Notes = "Thanh dẫn nhiệt đến VRM và VRAM."
                        },
                        new Component
                        {
                            Name = "Pin 95Wh",
                            MeshName = "Battery95Wh",
                            Material = "Lithium Polymer",
                            Description = "Pin dung lượng lớn hỗ trợ sạc nhanh 200W.",
                            Weight = 0.20,
                            Notes = "Chế độ Eco tự động khi pin < 20%."
                        }
                    }
                },
                new Product
                {
                    Name = "AeroVision Drone Pro",
                    Slug = "aerovision-drone-pro",
                    Description = "Drone quay phim chuyên nghiệp với camera 8K và hệ thống tránh vật cản 360°.",
                    ModelUrl = "/models/aerovision-pro.glb",
                    ThumbnailUrl = "/images/aerovision-thumb.jpg",
                    Weight = 1.12,
                    Material = "Sợi carbon",
                    Category = "Drone",
                    Manufacturer = "AeroVision Labs",
                    Components = new List<Component>
                    {
                        new Component
                        {
                            Name = "Khung drone",
                            MeshName = "DroneFrame",
                            Material = "Carbon fiber reinforced",
                            Description = "Khung chính với độ cứng cao và trọng lượng nhẹ.",
                            Weight = 0.48,
                            Notes = "Chịu được gió cấp 7."
                        },
                        new Component
                        {
                            Name = "Cánh quạt low-noise",
                            MeshName = "PropellerSet",
                            Material = "Nhựa composite",
                            Description = "Cánh quạt cánh én giảm tiếng ồn 25%.",
                            Weight = 0.08,
                            Notes = "Hệ thống gắn nhanh QuickMount."
                        },
                        new Component
                        {
                            Name = "Camera gimbal 8K",
                            MeshName = "CameraGimbal",
                            Material = "Magie + hợp kim nhôm",
                            Description = "Camera 8K với gimbal 3 trục chống rung.",
                            Weight = 0.22,
                            Notes = "Hỗ trợ ống kính 35mm interchangeable."
                        },
                        new Component
                        {
                            Name = "Pin bay 7800mAh",
                            MeshName = "FlightBattery",
                            Material = "Lithium Ion",
                            Description = "Thời gian bay tối đa 42 phút.",
                            Weight = 0.24,
                            Notes = "Hỗ trợ sạc nhanh 120W."
                        },
                        new Component
                        {
                            Name = "Module radar tránh vật cản",
                            MeshName = "RadarModule",
                            Material = "Nhựa ABS chống nước",
                            Description = "Radar 360° với AI nhận diện vật thể.",
                            Weight = 0.10,
                            Notes = "Tích hợp cảm biến cảm biến lidar."
                        }
                    }
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}

