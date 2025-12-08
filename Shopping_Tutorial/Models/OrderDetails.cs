using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OrderCode { get; set; }
        public long ProductID { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("ProductID")]
        public ProductModel Product { get; set; }



    }
}
