using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;
using AdminEmailSender = Shopping_Tutorial.Areas.Admin.Repository.IEmailSender;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Models.MoMo;
using Shopping_Tutorial.Services.Vnpay;

namespace Shopping_Tutorial.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly AdminEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;

        public CheckoutController(DataContext context, AdminEmailSender emailSender, IMomoService momoService, IVnPayService vnPayService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }
        private async Task ClearUserCartFromDatabase(string userId)
        {
            var userCartItems = _dataContext.UserCartItems
                                            .Where(c => c.UserId == userId);
            _dataContext.UserCartItems.RemoveRange(userCartItems);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId, string ContactUser)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                //Nhận Coupon code từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];
                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                else
                {
                    shippingPrice = 0;
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.UserName = userEmail;

                // Lấy ContactUser từ session nếu là thanh toán online
                if (PaymentMethod == "MOMO" || PaymentMethod == "VnPay")
                {
                    orderItem.ContactUser = HttpContext.Session.GetString("ContactUser");
                    // Xóa ContactUser khỏi session sau khi sử dụng
                    HttpContext.Session.Remove("ContactUser");
                }
                else
                {
                    orderItem.ContactUser = ContactUser;
                }

                if (PaymentMethod != "MOMO" && PaymentMethod != "VnPay")
                {
                    orderItem.PaymentMethod = PaymentMethod;
                }
                else if (PaymentMethod == "VnPay")
                {
                    orderItem.PaymentMethod = "VnPay " + PaymentId;
                }
                else if (PaymentMethod == "MOMO")
                {
                    orderItem.PaymentMethod = "Momo " + PaymentId;
                }

                orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                _dataContext.Add(orderItem);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItems = await _dataContext.UserCartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                foreach (var cart in cartItems)
                {
                    var orderdetails = new OrderDetails
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductID = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity
                    };

                    var product = await _dataContext.Products
                        .FirstOrDefaultAsync(p => p.Id == cart.ProductId);

                    if (product != null)
                    {
                        product.Quantity -= cart.Quantity;
                        product.Sold += cart.Quantity;
                    }

                    _dataContext.OrderDetails.Add(orderdetails);
                }

                await _dataContext.SaveChangesAsync();
                if (!string.IsNullOrEmpty(userId))
                {
                    await ClearUserCartFromDatabase(userId);
                }
                HttpContext.Session.Remove("Cart");

                // Xóa giỏ hàng của người dùng khỏi DB nếu có lưu trong UserCartItem
             

                //gửi maill thành công
                var receiver = userEmail;
                var subject = "Shop thương mại điện tử Quang Hùng";
                var message = "Bạn đã đặt hàng thành công tại shop .";

                await _emailSender.SendEmailAsync(receiver, subject, message);
                TempData["success"] = "Đơn hàng đã được tạo thành công và gửi thông báo đến mail của bạn, vui lòng chờ duyệt đơn hàng ";
                return RedirectToAction("History", "Account");
            }

            return View();
        }





        [HttpGet]
        public async Task<IActionResult> PaymentCallback(MomoInforModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            // Get ContactUser from the query string or session
            string contactUser = requestQuery["ContactUser"]; // or use Session if you stored it there before

            if (requestQuery["errorCode"] == "0")
            {
                var newMomoInsert = new MomoInforModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["Amount"]),
                    OrderInfor = requestQuery["orderInfor"],
                    DatePaid = DateTime.Now
                };
                _dataContext.Add(newMomoInsert);
                await _dataContext.SaveChangesAsync();

                // Call Checkout with the correct parameters
                var PaymentMethod = "MOMO";
                await Checkout(PaymentMethod, requestQuery["orderId"], contactUser);

                return View(response);
            }
            else
            {
                TempData["error"] = "Giao dịch MOMO không thành công !";
                return RedirectToAction("Index", "Cart");
            }
        }






        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Get ContactUser from the query string or session
            string contactUser = Request.Query["ContactUser"]; // or use Session if stored there

            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };
                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();

                // Call Checkout with the correct parameters
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId, contactUser);
            }
            else
            {
                TempData["error"] = "Giao dịch VNPAY không thành công !";
                return RedirectToAction("Index", "Cart");
            }

            return View(response);
        }






    }



}

