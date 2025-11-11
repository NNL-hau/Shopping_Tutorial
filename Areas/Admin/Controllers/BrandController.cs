using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{


    [Area("Admin")]
    [Route("Admin/Brand/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]

    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;

        }
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        //}

        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brand = _dataContext.Brands.ToList();


            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = brand.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
      
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {


            if (ModelState.IsValid)
            {
                // Tạo slug từ tên sản phẩm
                brand.Slug = brand.Name.Replace(" ", "-");

                // Kiểm tra xem slug đã tồn tại chưa
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong danh sách");
                    return View(brand);
                }



                // Thêm sản phẩm vào cơ sở dữ liệu
                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();

                // Hiển thị thông báo thành công
                TempData["success"] = "Thêm thương hiệu thành công";

                // Chuyển hướng về trang danh sách sản phẩm
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Nếu có lỗi trong model, hiển thị lại thông báo lỗi
                TempData["error"] = "Model có thể đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


        }
      
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }

       
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(brand);
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thương hiệu đã xóa";
            return RedirectToAction(nameof(Index));
        }
    }
}
