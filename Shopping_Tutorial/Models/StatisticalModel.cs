using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; } // so luong ban
        public int Sold { get; set; }//so luong don hang
        public decimal Revenue { get; set; } //so luong doanh thu
        public decimal Profit { get; set; } // loi nhuân
        public DateTime DateCreated { get; set; } //ngay dat hang

    }
}
