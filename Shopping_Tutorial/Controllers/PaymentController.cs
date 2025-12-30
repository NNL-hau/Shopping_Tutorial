using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        
        private IMomoService _momoService;
        private  IVnPayService _vnPayService;
        private readonly DataContext _dataContext;
        public PaymentController(IMomoService momoService, IVnPayService vnPayService, DataContext dataContext)
        {
            _vnPayService = vnPayService;
            _momoService = momoService;
            _dataContext = dataContext;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
        {
            // Lưu ContactUser vào session để sử dụng sau khi thanh toán
            HttpContext.Session.SetString("ContactUser", model.ContactUser);
            var response = await _momoService.CreatePaymentMomo(model);
            return Redirect(response.PayUrl);
        }
        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            // Lưu ContactUser vào session để sử dụng sau khi thanh toán
            HttpContext.Session.SetString("ContactUser", model.ContactUser);
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var couponCode = Request.Cookies["CouponTitle"];
            var coupon = _dataContext.Coupons.Where(c => couponCode.Contains(c.Name)).FirstOrDefault();
            if (coupon != null && coupon.Quantity > 0)
            {
                coupon.Quantity -= 1;
            }
            await _dataContext.SaveChangesAsync();
            return View(response);
        }
       
    }
}