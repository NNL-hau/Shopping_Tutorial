using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        
        private IMomoService _momoService;
        private  IVnPayService _vnPayService;
        public PaymentController(IMomoService momoService, IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
            _momoService = momoService;
            
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
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
       
    }
}