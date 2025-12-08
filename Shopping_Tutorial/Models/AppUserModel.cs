    using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Shopping_Tutorial.Models
{
    public class AppUserModel : IdentityUser
    {
        internal readonly bool IsActive;

        public string Occupation { get; set; }
        public string RoleId {  get; set; }
        public string Token { get; set; }

    }
}
