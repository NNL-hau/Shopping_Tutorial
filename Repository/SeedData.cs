using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();

            if (!_context.Products.Any())
            {
                // Thêm danh mục vào database
                CategoryModel macbook = new CategoryModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is Large Brand in the world", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "PC", Slug = "pc", Description = "PC is Large Brand in the world", Status = 1 };
                CategoryModel tai = new CategoryModel { Name = "Tai nghe", Slug = "tai-nghe", Description = "Thiết bị dùng để nghe ở tai", Status = 1 };
                CategoryModel chuot = new CategoryModel { Name = "Chuột Gigabyte", Slug = "chuot-gigabyte", Description = "Thiết bị dùng để điều khiển máy tính", Status = 1 };

                // Thêm thương hiệu vào database
                BrandModel apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is Large Brand in the world", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung is Large Brand in the world", Status = 1 };
                BrandModel gigabyte = new BrandModel { Name = "Gigabyte", Slug = "gigabyte", Description = "Gigabyte is Large Brand in the world", Status = 1 };
                BrandModel hh = new BrandModel { Name = "Hoàng Hà Mobile", Slug = "hoang-ha", Description = "Thương hiệu số 1 Việt Nam", Status = 1 };

                // Lưu danh mục và thương hiệu vào database
                _context.Categories.AddRange(macbook, pc, tai, chuot);
                _context.Brands.AddRange(apple, samsung, gigabyte, hh);
                _context.SaveChanges();

                // Thêm sản phẩm vào database
                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is best", Image = "1.jpg", Category = macbook, Brand = apple, Price = 12233000 },
                    new ProductModel { Name = "PC", Slug = "pc", Description = "PC is best", Image = "2.jpg", Category = pc, Brand = samsung, Price = 12233000 },
                    new ProductModel { Name = "PC 2024", Slug = "pc-2024", Description = "PC 2024 is best", Image = "5.jpg", Category = pc, Brand = gigabyte, Price = 23233000 },
                    new ProductModel { Name = "Macbook Vip", Slug = "macbook-vip", Description = "Macbook Vip is best", Image = "3.jpg", Category = macbook, Brand = hh, Price = 14433000 },
                    new ProductModel { Name = "Macbook 2019", Slug = "macbook-2019", Description = "Macbook 2019 is best", Image = "4.jpg", Category = macbook, Brand = samsung, Price = 14433000 },
                    new ProductModel { Name = "Chuột Gigabyte", Slug = "chuot-gigabyte", Description = "Chuột Gigabyte tốt nhất Việt Nam", Image = "5.png", Category = chuot, Brand = gigabyte, Price = 14433000 }
                    
                );
                // Thêm sliders vào database
                _context.Sliders.AddRange(
                    new SliderModel { Name = "Slider 1", Description = "Mô tả slider 1", Status = 1, Image = "banner1.png" },
                    new SliderModel { Name = "Slider 2", Description = "Mô tả slider 2", Status = 1, Image = "banner2.png" },
                    new SliderModel { Name = "Slider 3", Description = "Mô tả slider 3", Status = 1, Image = "banner3.png" },
                    new SliderModel { Name = "Slider 4", Description = "Mô tả slider 4", Status = 1, Image = "banner4.png" }
                );

                _context.SaveChanges();
            }

            if (!_context.Roles.Any())
            {
                // Create roles
                var roles = new List<IdentityRole>
                    {
                        new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                        new IdentityRole { Name = "Author", NormalizedName = "AUTHOR" },
                        new IdentityRole { Name = "Publisher", NormalizedName = "PUBLISHER" }
                    };

                foreach (var role in roles)
                {
                    _context.Roles.Add(role);
                }

                _context.SaveChanges();
            }

            if (!_context.Users.Any())
            {
                var user = new AppUserModel
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true, 
                    NormalizedUserName = "ADMIN",
                    NormalizedEmail = "ADMIN@GMAIL.COM",

                    PasswordHash = new PasswordHasher<AppUserModel>().HashPassword(null, "Ta!123"),
                    SecurityStamp = Guid.NewGuid().ToString(), 
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                _context.Users.Add(user);
                var roleName = "Admin";
                var role = _context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                {
                    role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };
                    _context.Roles.Add(role);
                    _context.SaveChanges();
                }
                var userRole = new IdentityUserRole<string>
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };
                _context.UserRoles.Add(userRole);
                _context.SaveChanges();

                Console.WriteLine("User and role assignment seeded successfully!");
            }
            else
            {
                Console.WriteLine("Users already exist in the database.");
            }
        }
    }
}
