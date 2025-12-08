using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class MomoInforModel
    {
        [Key]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string OrderInfor { get; set; }
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
        

    }
}
