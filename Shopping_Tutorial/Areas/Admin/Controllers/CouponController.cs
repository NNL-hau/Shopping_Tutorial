using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Coupon/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]
    public class CouponController:Controller
    {
        private readonly DataContext _dataContext;
        public CouponController(DataContext context)
        {
            _dataContext = context;

        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var coupon_list = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)
        {
            coupon.GiaNhap /= 100; // Chuyển đổi phần trăm thành số thập phân

            if (ModelState.IsValid)
            {

                _dataContext.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm coupon thành công";
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
            //return View();
        }
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var coupon = await _dataContext.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Không tìm thấy mã giảm giá." });
            }

            coupon.Status = status;
            _dataContext.Coupons.Update(coupon);
            await _dataContext.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
