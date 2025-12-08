using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/ChatAdmin/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]
    public class ChatAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
