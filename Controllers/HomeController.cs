using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;

        }

        public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "", int page = 1)
        {
            var products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable(); // Chuy?n sang IQueryable ?? c� th? l?c v� s?p x?p

            // Ki?m tra s? l??ng s?n ph?m
            var count = await products.CountAsync();
            if (count > 0)
            {
                // S?p x?p theo l?a ch?n
                if (sort_by == "price_increase")
                {
                    products = products.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    products = products.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    products = products.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    products = products.OrderBy(p => p.Id);
                }
                else
                {
                    products = products.OrderByDescending(p => p.Id);
                }

                // L?c theo gi�
                if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
                {
                    if (decimal.TryParse(startprice, out decimal startPriceValue) && decimal.TryParse(endprice, out decimal endPriceValue))
                    {
                        products = products.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                }
            }

            // Pagination 8 items per page
            const int pageSize = 6;
            int totalItems = await products.CountAsync();
            if (page < 1) page = 1;

            // Tìm sản phẩm bán chạy nhất (toàn bộ)
            var bestSellingProductId = await _dataContext.Products
                .Where(p => p.Sold > 0)
                .OrderByDescending(p => p.Sold)
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();

            var pagination = new Paginate(totalItems, page, pageSize);

            List<ProductModel> productsList;

            if (bestSellingProductId.HasValue)
            {
                var productsWithoutBest = products.Where(p => p.Id != bestSellingProductId.Value);

                if (pagination.CurrentPage == 1)
                {
                    var bestProduct = await products.FirstOrDefaultAsync(p => p.Id == bestSellingProductId.Value);
                    var rest = await productsWithoutBest
                        .Take(pageSize - 1)
                        .ToListAsync();
                    productsList = new List<ProductModel>();
                    if (bestProduct != null)
                    {
                        productsList.Add(bestProduct);
                    }
                    productsList.AddRange(rest);
                }
                else
                {
                    int skip = (pagination.CurrentPage - 1) * pageSize - 1; // trừ 1 vì trang 1 đã lấy bớt 1 sp (best)
                    if (skip < 0) skip = 0;
                    productsList = await productsWithoutBest
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
            }
            else
            {
                productsList = await products
                    .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();
            }
            var sliders = await _dataContext.Sliders.Where(s => s.Status == 1).ToListAsync();

            ViewBag.Sliders = sliders;
            ViewBag.BestSellingProductId = bestSellingProductId;
            ViewBag.Pagination = pagination;
            ViewBag.SortBy = sort_by;
            ViewBag.StartPrice = startprice;
            ViewBag.EndPrice = endprice;

            return View(productsList);
        }

        public async Task<IActionResult> Compare()
        {
            var user = await _userManager.GetUserAsync(User);
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductID equals p.Id
                                         where c.UserId == user.Id
                                         select new { Product = p, Compares = c })
                               .ToListAsync();

            return View(compare_product);
        }
        [HttpGet]
        public async Task<IActionResult> CompareMany([FromQuery(Name = "ids")] int[] ids)
        {
            if (ids == null || ids.Length < 2 || ids.Length > 5)
            {
                TempData["error"] = "Vui lòng chọn tối thiểu 2 và tối đa 5 sản phẩm để so sánh.";
                return RedirectToAction("Compare");
            }

            var compareItems = await _dataContext.Compares
                .Where(c => ids.Contains(c.Id))
                .Include(c => c.Product)
                    .ThenInclude(p => p.Brand)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .ToListAsync();

            if (compareItems.Count != ids.Length)
            {
                TempData["error"] = "Một số sản phẩm không tồn tại trong danh sách so sánh.";
                return RedirectToAction("Compare");
            }

            var model = new Shopping_Tutorial.Models.ViewModels.CompareManyViewModel
            {
                Products = compareItems.Select(c => c.Product).ToList()
            };

            // Save comparison history
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var productNames = string.Join(", ", compareItems.Select(c => c.Product.Name));
                var compareHistory = new CompareHistoryModel
                {
                    UserId = user.Id,
                    ComparedProductNames = productNames,
                    ComparisonDate = DateTime.Now
                };
                _dataContext.CompareHistories.Add(compareHistory);
                await _dataContext.SaveChangesAsync();
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> CompareTwo(int firstId, int secondId)
        {
            var firstCompare = await _dataContext.Compares
                .Include(c => c.Product)
                    .ThenInclude(p => p.Brand)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == firstId);

            var secondCompare = await _dataContext.Compares
                .Include(c => c.Product)
                    .ThenInclude(p => p.Brand)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == secondId);

            if (firstCompare == null || secondCompare == null)
            {
                return NotFound();
            }

            var model = new Shopping_Tutorial.Models.ViewModels.CompareTwoViewModel
            {
                ProductA = firstCompare.Product,
                ProductB = secondCompare.Product
            };

            return View(model);
        }
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);

            _dataContext.Compares.Remove(compare);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "X�a th�nh c�ng !";
            return RedirectToAction("Compare", "Home");
        }
        public async Task<IActionResult> Wishlist()
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductID equals p.Id
                                          where w.UserId == user.Id
                                          select new { Product = p, Wishlists = w })
                               .ToListAsync();

            return View(wishlist_product);
        }
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

            _dataContext.Wishlists.Remove(wishlist);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "X�a th�nh c�ng !";
            return RedirectToAction("Wishlist", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(long Id, WishlistModel wishlistmodel)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductID = Id,
                UserId = user.Id
            };

            _dataContext.Wishlists.Add(wishlistProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Th�m s?n ph?m v�o danh s�ch y�u th�ch th�nh c�ng !" });
            }
            catch (Exception)
            {
                return StatusCode(500, "C� l?i x?y ra khi th�m v�o danh s�ch y�u th�ch");
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddCompare(long Id)
        {
            var user = await _userManager.GetUserAsync(User);

            var compareProduct = new CompareModel
            {
                ProductID = Id,
                UserId = user.Id
            };

            _dataContext.Compares.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Th�m s?n ph?m v�o danh s�ch so s�nh th�nh c�ng!" });
            }
            catch (Exception)
            {
                return StatusCode(500, "C� l?i x?y ra khi th�m v�o danh s�ch so s�nh !");
            }

        }


        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Homepage()
        {
            return View();
        }
        public IActionResult TestEffect()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contact.FirstAsync();
            return View(contact);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
