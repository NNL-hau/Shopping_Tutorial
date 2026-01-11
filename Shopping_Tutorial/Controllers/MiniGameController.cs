using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;
using System.Threading.Tasks;
using System.Linq;

namespace Shopping_Tutorial.Controllers
{
    public class MiniGameController : Controller
    {
        private readonly DataContext _dataContext;

        public MiniGameController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _dataContext.Products
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Play()
        {
            var count = await _dataContext.Products.CountAsync();
            if (count == 0) return RedirectToAction("Index");
            var rnd = new System.Random();
            var skip = rnd.Next(0, count);
            var product = await _dataContext.Products.Skip(skip).Take(1).FirstOrDefaultAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Random()
        {
            var count = await _dataContext.Products.CountAsync();
            if (count == 0) return Ok(new { success = false, message = "Không có sản phẩm" });
            var rnd = new System.Random();
            var skip = rnd.Next(0, count);
            var product = await _dataContext.Products.Skip(skip).Take(1)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    image = p.Image,
                    brand = p.Brand != null ? p.Brand.Name : null,
                    category = p.Category != null ? p.Category.Name : null
                })
                .FirstOrDefaultAsync();
            return Ok(new { success = true, product });
        }
    }
}

