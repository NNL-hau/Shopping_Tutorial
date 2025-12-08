using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Migrations;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]


    public class OrderController:Controller 
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;

        }
        public async Task<IActionResult> Index()
        {
            
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
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



        [HttpGet]
        [Route("PaymemtMoMoInfo")]
        public async Task<IActionResult> PaymemtMoMoInfo(string orderId)
        {
            var momoInfo = await _dataContext.MomoInfors.FirstOrDefaultAsync(m => m.OrderId == orderId);
            if(momoInfo == null)
            {
                return NotFound();
            }
            return View(momoInfo);
        }




        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _dataContext.Update(order);
            if(status ==2)
            {
                var DetailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice
                    }).ToListAsync();

                // Get coupon discount if exists
                var couponCode = Request.Cookies["CouponTitle"];
                var discountValue = 0m;
                if (couponCode != null)
                {
                    var discountValueCookie = Request.Cookies["DiscountValue"];
                    if (discountValueCookie != null)
                    {
                        discountValue = JsonConvert.DeserializeObject<decimal>(discountValueCookie);
                    }
                }

                var statistiaclModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date==order.CreatedDate.Date);
                if (statistiaclModel != null)
                {
                    foreach (var orderDetail in DetailsOrder)
                    {
                        statistiaclModel.Quantity += orderDetail.Quantity;
                        statistiaclModel.Sold += orderDetail.Quantity;
                        // Calculate revenue after discount
                        decimal totalBeforeDiscount = orderDetail.Quantity * orderDetail.Price;
                        decimal discountAmount = totalBeforeDiscount * discountValue;
                        decimal revenueAfterDiscount = totalBeforeDiscount - discountAmount;
                        statistiaclModel.Revenue += revenueAfterDiscount;
                        // Calculate profit: (Price - CapitalPrice) * Quantity - DiscountAmount
                        decimal capitalCost = orderDetail.CapitalPrice * orderDetail.Quantity;
                        statistiaclModel.Profit += revenueAfterDiscount - capitalCost;
                    }
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    decimal new_revenue = 0;

                    foreach (var orderDetail in DetailsOrder)
                    {
                        new_quantity += orderDetail.Quantity;
                        new_sold += orderDetail.Quantity;
                        // Calculate revenue after discount
                        decimal totalBeforeDiscount = orderDetail.Quantity * orderDetail.Price;
                        decimal discountAmount = totalBeforeDiscount * discountValue;
                        decimal revenueAfterDiscount = totalBeforeDiscount - discountAmount;
                        new_revenue += revenueAfterDiscount;
                        // Calculate profit: (Price - CapitalPrice) * Quantity - DiscountAmount
                        decimal capitalCost = orderDetail.CapitalPrice * orderDetail.Quantity;
                        new_profit += revenueAfterDiscount - capitalCost;
                    }

                    statistiaclModel = new StatisticalModel
                    {
                        DateCreated = order.CreatedDate,
                        Quantity = new_quantity,
                        Sold = new_sold,
                        Revenue = new_revenue,
                        Profit = new_profit
                    };

                    _dataContext.Add(statistiaclModel);
                }
            }

           

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {

                //delete order
                _dataContext.Orders.Remove(order);


                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

    }
}
