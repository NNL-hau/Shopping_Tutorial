using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;
using AdminEmailSender = Shopping_Tutorial.Areas.Admin.Repository.IEmailSender;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using System.Security.Claims;
using Shopping_Tutorial.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;

namespace Shopping_Tutorial.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly AdminEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(AdminEmailSender emailSender, SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManage, DataContext context)
        {
            _signInManager = signInManager;
            _userManage = userManage;
            _emailSender = emailSender;
            _dataContext = context;
        }

        public IActionResult Login(string returnUrl)
        {
            // Nếu không có returnUrl, mặc định quay về Home/Index
            var target = string.IsNullOrWhiteSpace(returnUrl)
                ? Url.Action("Index", "Home")
                : returnUrl;
            return View(new LoginViewModel { ReturnUrl = target });
        }
        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            // Create AppUserModel from UserModel
            var appUserModel = new AppUserModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash=user.PasswordHash
                // Map other properties from UserModel to AppUserModel
            };

            return View(appUserModel);  // Pass AppUserModel to the view
        }
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            var userById = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
               
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);

                userById.PasswordHash = passwordHash;
                

                _dataContext.Update(userById);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa thông tin thành công";
            }
            

            return RedirectToAction("UpdateAccount","Account");  
        }


        [HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

			if (checkMail == null)
			{
				TempData["error"] = "Không tìm thấy Email";
				return RedirectToAction("ForgotPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();
				var receiver = checkMail.Email;
				var subject = "Tạo mật khẩu mới " + checkMail.Email;
				var message = "Nhấp vào link để đổi mật khẩu " +
					"<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "'>";

				await _emailSender.SendEmailAsync(receiver, subject, message);
			}


			TempData["success"] = "Một Email đã gửi về mail của bạn, hãy kiểm tra mail để đổi lại mật khẩu của bạn!";
			return RedirectToAction("ForgotPass", "Account");
		}
		public IActionResult ForgotPass()
		{
			return View();
		}
        public async Task<IActionResult> NewPass(string returnUrl, AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Mã gửi đổi mật khẩu không đúng";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                //update user with new password and token
                string newtoken = Guid.NewGuid().ToString();
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid) // Đảm bảo model hợp lệ trước khi kiểm tra đăng nhập
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(loginVM.ReturnUrl))
                    {
                        // Đảm bảo chỉ redirect nội bộ để tránh open redirect
                        return LocalRedirect(loginVM.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hoặc tài khoản không đúng");
                }
            }
            return View(loginVM); // Nếu có lỗi, trả về View và hiển thị thông báo lỗi
        }

        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account"); // Thay thế "Account" bằng tên controller của bạn
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách đơn hàng
            var orders = await _dataContext.Orders
                .Where(od => od.UserName == userEmail)
                .OrderByDescending(od => od.Id)
                .ToListAsync();

            // Lấy chi tiết đơn hàng (giả sử có bảng order details)
            var orderDetails = await _dataContext.OrderDetails
                .Where(od => orders.Select(o => o.Id).Contains(od.Id))
                .ToListAsync();

            // Tạo ViewModel để chứa cả Orders và OrderDetails
            var viewModel = new OrderViewModel
            {
                Orders = orders,
                OrderDetails = orderDetails
            };

            ViewBag.UserEmail = userEmail;
            return View(viewModel);
        }



        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                //Kiểm tra đăng nhập
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return BadRequest("Có lỗi khi hủy đơn hàng .");
            }


            return RedirectToAction("History", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.UserName, Email = user.Email };
                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công";
                    // Sau khi tạo tài khoản, điều hướng tới trang đăng nhập và yêu cầu quay về Home/Index sau khi đăng nhập
                    return RedirectToAction("Login", new { returnUrl = Url.Action("Index", "Home") });
                }
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }
        public async Task<IActionResult> Logout(String returnUrl = "/")
        {
            
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl);
        }
        public async Task LoginByGoogle()
        {
            // Use Google authentication scheme for challenge
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }
        public async Task<IActionResult>
         GoogleResponse()
        {
            // Authenticate using Google scheme
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            //var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            string emailName = email.Split('@')[0];
            //return Json(claims);
            // Check user có tồn tại không
            var existingUser = await _userManage.FindByEmailAsync(email);

            if (existingUser == null)
            {
                //nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");
                //username thay khoảng cách bằng dấu "-" và chữ thường hết
                var newUser = new AppUserModel { UserName = emailName, Email = email };
                newUser.PasswordHash = hashedPassword; // Set the hashed password cho user

                var createUserResult = await _userManage.CreateAsync(newUser);
                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

                }
                else
                {
                    // Nếu user tạo user thành công thì đăng nhập luôn 
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng ký tài khoản thành công.";
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                //Còn user đã tồn tại thì đăng nhập luôn với existingUser
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }

            return RedirectToAction("Index","Home");

        }
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                return NotFound("Mã đơn hàng không hợp lệ.");
            }

            // Lấy chi tiết đơn hàng
            var detailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            // Lấy thông tin đơn hàng
            var order = await _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .FirstOrDefaultAsync();

            if (order == null || detailsOrder == null || !detailsOrder.Any())
            {
                return NotFound("Không tìm thấy thông tin chi tiết đơn hàng.");
            }
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

            // Tạo OrderViewModel và trả về View
            var orderViewModel = new OrderViewModel
            {
                Orders = new List<OrderModel> { order }, // Danh sách Order, chỉ có một đơn hàng
                OrderDetails = detailsOrder,
                AppliedCoupon = new CouponModel
                {
                    // Assuming you have a way to fetch the coupon details
                    Name = couponCode,
                    GiaNhap = discountValue
                }

            };

            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.Status = order.Status;

            return View(orderViewModel);
        }

        #region API Endpoint
        [HttpGet("/api/get/users")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userManage.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("/api/get/user/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManage.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        #endregion
    }
}
