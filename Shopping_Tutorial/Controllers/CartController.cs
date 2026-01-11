using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using Microsoft.AspNetCore.Identity;
namespace Shopping_Tutorial.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private UserManager<AppUserModel> _userManager;


        public CartController(DataContext context, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(ShippingModel shippingModel)
        {
            // Nhận shipping giá từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            var cartItems = await _dataContext.UserCartItems
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            // Lấy thông tin mã giảm giá từ cookie
            var couponCode = Request.Cookies["CouponTitle"];
            var discountValue = 0m; // Default value for discount

            if (couponCode != null)
            {
                // Assuming the coupon value is also stored in the cookie
                var discountValueCookie = Request.Cookies["DiscountValue"];
                if (discountValueCookie != null)
                {
                    discountValue = JsonConvert.DeserializeObject<decimal>(discountValueCookie);
                }
            }
            var coinDiscountCookie = Request.Cookies[$"CoinDiscountValue_{user.Id}"];
            var coinDiscountAmount = 0m;
            if (coinDiscountCookie != null)
            {
                coinDiscountAmount = JsonConvert.DeserializeObject<decimal>(coinDiscountCookie);
            }

            var availableCoupons = await _dataContext.Coupons
.Where(c => c.DateStart <= DateTime.Now && c.DateExpired >= DateTime.Now && c.Quantity > 0 && c.Status == 1 && !EF.Functions.Like(c.Name, "WHEEL-%"))
.ToListAsync();

            var wheelCoupons = await _dataContext.Coupons
.Join(_dataContext.LuckySpinHistories.Where(h => h.UserId == user.Id && h.CouponCode != null),
      c => c.Name,
      h => h.CouponCode,
      (c, h) => c)
.Where(c => c.DateStart <= DateTime.Now && c.DateExpired >= DateTime.Now && c.Quantity > 0 && c.Status == 1 && EF.Functions.Like(c.Name, "WHEEL-%"))
.ToListAsync();


            var userCoin = await _dataContext.UserCoins.FirstOrDefaultAsync(x => x.UserId == user.Id);

            var cartVM = new CartItemViewModel
            {
                CartItems = cartItems.Select(x => new CartItemModel
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    Image = x.Image
                }).ToList(),
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingPrice = shippingPrice,
                CouponCode = couponCode,
                AppliedCoupon = new CouponModel
                {
                    Name = couponCode,
                    GiaNhap = discountValue
                },
                AvailableCoupons = availableCoupons,
                WheelCoupons = wheelCoupons,
                UserCoins = userCoin?.Coins ?? 0,
                CoinDiscountAmount = coinDiscountAmount
            };

            return View(cartVM);
        }





        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> Add(long Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _dataContext.Products.FindAsync(Id);
            var existingItem = await _dataContext.UserCartItems
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProductId == Id);

            if (existingItem == null)
            {
                var newItem = new UserCartItem
                {
                    UserId = user.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    Image = product.Image
                };

                _dataContext.UserCartItems.Add(newItem);
            }
            else
            {
                existingItem.Quantity++;
                _dataContext.UserCartItems.Update(existingItem);
            }

            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Sản phẩm đã được thêm vào giỏ hàng của bạn";
            return Redirect(Request.Headers["Referer"].ToString());
        }


        public async Task<IActionResult> Decrease(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _dataContext.UserCartItems
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProductId == Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    _dataContext.UserCartItems.Update(cartItem);
                }
                else
                {
                    _dataContext.UserCartItems.Remove(cartItem);
                }
                await _dataContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Increase(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
            var cartItem = await _dataContext.UserCartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == Id);

            if (cartItem != null && product != null)
            {
                if (cartItem.Quantity < product.Quantity)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    TempData["error"] = "Không thể tăng số lượng vì đã đạt tối đa tồn kho.";
                }

                _dataContext.UserCartItems.Update(cartItem);
                await _dataContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        // Hàm xóa sản phẩm khỏi giỏ hàng
        public async Task<IActionResult> Remove(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _dataContext.UserCartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == Id);

            if (cartItem != null)
            {
                _dataContext.UserCartItems.Remove(cartItem);
                await _dataContext.SaveChangesAsync();
            }

            TempData["success"] = "Bạn đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }


        // Hàm xóa tất cả sản phẩm trong giỏ hàng
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _dataContext.UserCartItems.Where(x => x.UserId == user.Id);
            _dataContext.UserCartItems.RemoveRange(cartItems);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Bạn đã xóa toàn bộ sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {

            var existingShipping = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

            decimal shippingPrice = 0; // Set mặc định giá tiền

            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                //Set mặc định giá tiền nếu ko tìm thấy
                shippingPrice = 50000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true // using HTTPS
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                //
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }
        [HttpGet]
        [Route("Cart/DeleteShipping")]
        public IActionResult DeleteShipping()
        {
            Response.Cookies.Delete("ShippingPrice");
            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var userId = _userManager.GetUserId(User);
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            if (validCoupon == null)
            {
                return Ok(new { success = false, message = "Mã giảm giá không tồn tại" });
            }

            if (validCoupon.Status != 1 || validCoupon.Quantity <= 0)
            {
                return Ok(new { success = false, message = "Mã giảm giá đã sử dụng hoặc không khả dụng" });
            }

            // ❗ THÊM: Kiểm tra nếu chưa đến ngày bắt đầu
            if (validCoupon.DateStart > DateTime.Now)
            {
                return Ok(new { success = false, message = "Mã giảm giá chưa đến ngày sử dụng" });
            }

            // ❗ GIỮ nguyên kiểm tra ngày hết hạn
            TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
            int daysRemaining = remainingTime.Days;

            if (daysRemaining >= 0)
            {
                try
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };

                    string couponTitle = validCoupon.Name + " | " + validCoupon.Description;
                    Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);

                    decimal discountValue = Math.Round(validCoupon.GiaNhap, 2);
                    var discountValueJson = JsonConvert.SerializeObject(discountValue);
                    Response.Cookies.Append("DiscountValue", discountValueJson, cookieOptions);

                    return Ok(new { success = true, message = "Áp mã giảm giá thành công" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding apply coupon cookie: {ex.Message}");
                    return Ok(new { success = false, message = "Áp mã giảm giá sai" });
                }
            }
            else
            {
                return Ok(new { success = false, message = "Mã giảm giá đã hết hạn" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RedeemCoins(int coins)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (coins <= 0)
            {
                return Ok(new { success = false, message = "Số xu đổi không hợp lệ" });
            }

            var userCoin = await _dataContext.UserCoins.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (userCoin == null || userCoin.Coins <= 0)
            {
                return Ok(new { success = false, message = "Bạn chưa có xu để đổi" });
            }

            if (coins > userCoin.Coins)
            {
                coins = userCoin.Coins;
            }

            var cartItems = await _dataContext.UserCartItems.Where(x => x.UserId == user.Id).ToListAsync();
            var grandTotal = cartItems.Sum(x => x.Quantity * x.Price);

            var couponValueCookie = Request.Cookies["DiscountValue"];
            var percent = 0m;
            if (couponValueCookie != null)
            {
                percent = JsonConvert.DeserializeObject<decimal>(couponValueCookie);
            }
            var afterPercent = grandTotal * (1 - percent);

            var redeemValuePerCoin = 10m; // 10 VND mỗi xu (100 xu = 1000đ)
            var desiredDiscount = coins * redeemValuePerCoin;
            var maxDiscountAllowed = afterPercent; // không vượt số tiền hàng sau khi áp mã
            var actualDiscount = Math.Min(desiredDiscount, maxDiscountAllowed);
            var coinsUsed = (int)Math.Floor(actualDiscount / redeemValuePerCoin);

            if (coinsUsed <= 0)
            {
                return Ok(new { success = false, message = "Giá trị đơn quá thấp để đổi xu" });
            }

            userCoin.Coins -= coinsUsed;
            userCoin.UpdatedAt = DateTime.UtcNow;
            await _dataContext.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            var discountJson = JsonConvert.SerializeObject(actualDiscount);
            Response.Cookies.Append($"CoinDiscountValue_{user.Id}", discountJson, cookieOptions);

            return Ok(new { success = true, message = $"Bạn đã đổi {coinsUsed} xu thành công. Giảm {actualDiscount:#,##0} đ" });
        }

        [HttpGet]
        public async Task<IActionResult> SmashPiggyBank()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new { success = false, message = "Vui lòng đăng nhập để đập heo đất" });
                }

                var smashedKey = $"PiggyBankSmashed_{user.Id}";
                var smashedCookie = Request.Cookies[smashedKey];
                if (smashedCookie != null)
                {
                    return Ok(new { success = false, message = "Bạn đã đập heo đất rồi" });
                }

                var rnd = new Random();
                if (rnd.NextDouble() < 0.3)
                {
                    var smashedOptionsUnlucky = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(365),
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Append(smashedKey, "true", smashedOptionsUnlucky);
                    return Ok(new { success = false, message = "Chúc bạn may mắn lần sau" });
                }

                var availableCoupons = await _dataContext.Coupons
                    .Where(c => c.DateStart <= DateTime.Now && c.DateExpired >= DateTime.Now && c.Quantity > 0 && c.Status == 1)
                    .ToListAsync();

                decimal discountValue;
                string couponTitle;

                if (availableCoupons.Any())
                {
                    var selected = availableCoupons[rnd.Next(availableCoupons.Count)];
                    discountValue = Math.Round(selected.GiaNhap, 2);
                    couponTitle = selected.Name + " | " + selected.Description;
                }
                else
                {
                    var randomPercent = rnd.Next(5, 11);
                    discountValue = Math.Round((decimal)randomPercent / 100m, 2);
                    couponTitle = "Heo đất | Giảm " + randomPercent + "%";
                }

                if (discountValue > 0.10m)
                {
                    discountValue = 0.10m;
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                var discountValueJson = JsonConvert.SerializeObject(discountValue);
                Response.Cookies.Append("DiscountValue", discountValueJson, cookieOptions);

                var smashedOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(365),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append(smashedKey, "true", smashedOptions);

                return Ok(new { success = true, message = "Bạn đã đập heo đất và nhận giảm giá", title = couponTitle, discount = discountValue });
            }
            catch
            {
                return Ok(new { success = false, message = "Không thể thực hiện đập heo đất" });
            }
        }




    }
}
