//dungf dder xem xdanh sach
namespace Shopping_Tutorial.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal {  get; set; }
        public decimal ShippingPrice { get; set; }
        public string CouponCode { get; set; }
        public CouponModel AppliedCoupon { get; set; } // ✅ Thêm dòng này

        public List<CouponModel> AvailableCoupons { get; set; } // ✨ thêm dòng này

    }
}
