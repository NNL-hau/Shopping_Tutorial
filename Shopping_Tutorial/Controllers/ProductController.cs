using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services;
using System.Linq;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IProductRecommendationService _recommendationService;

        // Constructor để inject DataContext vào controller
        public ProductController(DataContext context, IProductRecommendationService recommendationService)
        {
            _dataContext = context;
            _recommendationService = recommendationService;
        }

        // Phương thức trả về trang chính của controller
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Search(string searchTerm)
        {
            // Lưu lịch sử tìm kiếm nếu người dùng đã đăng nhập
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await _recommendationService.SaveSearchHistoryAsync(userId, searchTerm);
                }
            }

            var products = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(searchTerm) ||
                            p.Brand.Name.Contains(searchTerm) ||
                            p.Category.Name.Contains(searchTerm))
                .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }






        //Phương thức Details để lấy chi tiết sản phẩm theo Id
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra nếu id null, trả về trang Index
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Truy vấn sản phẩm với id và dùng FirstOrDefaultAsync để lấy dữ liệu bất đồng bộ
            var product = await _dataContext.Products.Include(p=>p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);


            var relatedProducts = await _dataContext.Products
            .Where(p => p.CategoryID == product.CategoryID && p.Id != product.Id)
            .Take(4)
            .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;


            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = product,
                Ratings = product.Ratings.ToList().OrderByDescending(r => r.Id).ToList()
            };



            // Kiểm tra nếu không tìm thấy sản phẩm, trả về trang Index
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            // Trả về view với thông tin sản phẩm
            return View(viewModel);
        }


   

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {

                var ratingEntity = new RatingModel
                {
                    ProductID = rating.ProductID,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star

                };

                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công";

                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Hãy nhập đủ thông tin để đánh giá";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);

                return RedirectToAction("Detail", new { id = rating.ProductID });
            }

            return Redirect(Request.Headers["Referer"]);
        }

        // 3D Viewer Action
        public async Task<IActionResult> View3D(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var product = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Product3D)
                .Include(p => p.Annotations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return RedirectToAction("Index");
            }

            return View(product);
        }
    }

}

