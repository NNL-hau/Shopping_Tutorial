    namespace Shopping_Tutorial.Models.ViewModels
    {
        public class OrderViewModel
        {
            public IEnumerable<OrderModel> Orders { get; set; }
            public IEnumerable<OrderDetails> OrderDetails { get; set; }
        public CouponModel AppliedCoupon { get; set; } // ✅ Thêm dòng này

    }
}
