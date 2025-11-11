namespace Shopping_Tutorial.Models
{
    public class UserCartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } // User Id từ Identity
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        public decimal Total => Quantity * Price;
    }

}
