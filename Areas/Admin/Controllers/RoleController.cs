using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role/[action]")]
    //[Authorize(Roles = "Admin")]

    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = context;
            _roleManager = roleManager;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]
       
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet]
        [Route("Edit")]

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Handle missing Id
            }
            var role = await _roleManager.FindByIdAsync(id);

            return View(role);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Handle missing Id
            }
            if (ModelState.IsValid) // Validate model state before proceeding
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(); // Handle role not found
                }
                role.Name = model.Name; // Update role properties with model data
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Quyền đã được update!";
                    return RedirectToAction("Index", "Role", new { area = "Admin" });
                    // Redirect to the index action
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi khi update.");
                }

            }
            return View(model ?? new IdentityRole { Id = id });
        }


        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(IdentityRole model)
        {
            //avoid duplicate role
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index", "Role", new { area = "Admin" });
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Handle missing Id
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(); // Handle role not found
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa quyền thành công";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Có 1 lỗi gì đó xuất hiện ở Role Delete.");
            }

            return RedirectToAction("Index", "Role", new { area = "Admin" });
        }
    }
}