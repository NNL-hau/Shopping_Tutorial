//dungf dder xem xdanh sach
namespace Shopping_Tutorial.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal ShippingPrice { get; set; }
        public string CouponCode { get; set; }
        public CouponModel AppliedCoupon { get; set; } // ✅ Thêm dòng này

        public List<CouponModel> AvailableCoupons { get; set; } // ✨ thêm dòng này
        public List<CouponModel> WheelCoupons { get; set; }

        public int UserCoins { get; set; }

        public decimal CoinDiscountAmount { get; set; }

    }
}

