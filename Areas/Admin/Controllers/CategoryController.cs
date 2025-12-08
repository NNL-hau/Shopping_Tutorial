using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{


    [Area("Admin")]
    [Route("Admin/Category/[action]")]
    //[Authorize(Roles = "Admin,Sales,Author")]

    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
           
        }
       
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }



        public async Task<IActionResult> Edit(int Id)
        {

            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);                                          
        }
        public IActionResult Create()
        {
         
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
          

            if (ModelState.IsValid)
            {
                // Tạo slug từ tên sản phẩm
               category.Slug = category.Name.Replace(" ", "-");

                // Kiểm tra xem slug đã tồn tại chưa
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug ==category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong danh sách");
                    return View(category);
                }

               

                // Thêm sản phẩm vào cơ sở dữ liệu
                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();

                // Hiển thị thông báo thành công
                TempData["success"] = "Thêm danh mục thành công";

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
         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
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
            return View(category);
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Danh mục đã xóa";
            return RedirectToAction(nameof(Index));
        }
    }
}
