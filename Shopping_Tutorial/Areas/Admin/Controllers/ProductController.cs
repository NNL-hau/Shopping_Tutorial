using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Globalization;
using System.IO;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Product/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        // Constructor nhận đối tượng DataContext
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Phương thức Index để lấy danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sản phẩm, sắp xếp theo Id giảm dần và include thông tin Category
            var products = await _dataContext.Products
                                              .Include(p => p.Category)
                                              .Include(p => p.Brand)// Bao gồm thông tin Category
                                              .OrderByDescending(p => p.Id) // Sắp xếp giảm dần theo Id
                                              .ToListAsync(); // Lấy dữ liệu bất đồng bộ

            return View(products); // Trả về view với danh sách sản phẩm
        }
        // Phương thức GET để hiển thị form tạo mới sản phẩm
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        // Phương thức POST để xử lý lưu sản phẩm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            // Nếu model không hợp lệ, trả lại view với lỗi
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

            if (ModelState.IsValid)
            {
                // Tạo slug từ tên sản phẩm
                product.Slug = product.Name.Replace(" ", "-");

                if (product.Model3DUpload != null)
                {
                    var modelPath = await Handle3DModelUpload(product.Model3DUpload);
                    if (modelPath != null)
                    {
                        product.Model3DLink = modelPath;
                        product.Product3D = new Product3DModel
                        {
                            Model3DPath = modelPath
                        };
                    }
                }
                else
                {
                    product.Model3DLink = NormalizeModel3DLink(product.Model3DLink);
                }

                // Kiểm tra xem slug đã tồn tại chưa
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong danh sách");
                    return View(product);
                }

                // Xử lý upload ảnh nếu có
                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");//sửa thành media product để quay lại file đầu
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Lưu ảnh vào thư mục
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fs);
                    }

                    product.Image = imageName; // Lưu tên ảnh vào sản phẩm
                }

                // Thêm sản phẩm vào cơ sở dữ liệu
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();

                // Hiển thị thông báo thành công
                TempData["success"] = "Thêm sản phẩm thành công";

                // Chuyển hướng về trang danh sách sản phẩm
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Nếu có lỗi trong model, hiển thị lại thông báo lỗi
                TempData["error"] = "Model có thể đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
           
        }
        

        public async Task<IActionResult> Edit(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

            return View(product);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel product)
        {
            var existed_product = _dataContext.Products.Find(product.Id); //tìm sp theo id product
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;
                }


                // Update other product properties
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.CapitalPrice = product.CapitalPrice;
                existed_product.Price = product.Price;
                existed_product.CategoryID = product.CategoryID;
                existed_product.BrandID = product.BrandID;

                if (product.Model3DUpload != null)
                {
                    var modelPath = await Handle3DModelUpload(product.Model3DUpload);
                    if (modelPath != null)
                    {
                        existed_product.Model3DLink = modelPath;

                        var product3D = await _dataContext.Product3DModels.FirstOrDefaultAsync(p => p.ProductID == existed_product.Id);
                        if (product3D == null)
                        {
                            product3D = new Product3DModel { ProductID = existed_product.Id, Model3DPath = modelPath };
                            _dataContext.Product3DModels.Add(product3D);
                        }
                        else
                        {
                            product3D.Model3DPath = modelPath;
                            product3D.UpdatedDate = DateTime.Now;
                            _dataContext.Update(product3D);
                        }
                    }
                }
                else
                {
                    existed_product.Model3DLink = NormalizeModel3DLink(product.Model3DLink);
                }
                // ... other properties
                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(product);
        }
        [Route("DeletePRD")]
        public async Task<IActionResult> DeletePRD (long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.Equals(product.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");//sửa thành media product để quay lại file đầu
                string oldfileImage = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction(nameof(Index));
        }
        //add more quantity
        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(long Id)
        {
           
            var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.Id = Id;
            return View(); // Trả về view với danh sách sản phẩm
        }

        [Route("StoreProductQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound(); // Handle product not found scenario
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateTime = DateTime.Now;


            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }

        private string? NormalizeModel3DLink(string? link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return null;
            }

            var trimmed = link.Trim();

            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("//"))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("~/"))
            {
                trimmed = trimmed.Substring(1);
            }

            // Normalize backslashes to forward slashes for storage
            trimmed = trimmed.Replace("\\", "/");

            if (Path.IsPathRooted(trimmed))
            {
                try
                {
                    // Try to make it relative to webroot if it's a local path
                    // We use the original link (or one with backslashes) for Path operations if needed, 
                    // but GetRelativePath handles both separators usually.
                    var relativePath = Path.GetRelativePath(_webHostEnvironment.WebRootPath, link.Trim());
                    if (!relativePath.StartsWith(".."))
                    {
                        return "/" + relativePath.Replace("\\", "/").TrimStart('/');
                    }
                }
                catch
                {
                    // ignore and continue with other normalization steps
                }
            }

            if (!trimmed.StartsWith("/"))
            {
                trimmed = "/" + trimmed.TrimStart('/');
            }

            return trimmed;
        }

        private async Task<string?> Handle3DModelUpload(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var validExtensions = new[] { ".glb", ".gltf", ".obj", ".fbx", ".3ds", ".mtl" };
            
            if (!validExtensions.Contains(extension))
            {
                return null;
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "3d");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return "/3d/" + fileName;
        }
    }
    
}
