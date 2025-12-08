using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Hãy nhập Username")]
        public string UserName { get; set; }
       
        [DataType(DataType.Password), Required(ErrorMessage = "Hãy nhập password")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
