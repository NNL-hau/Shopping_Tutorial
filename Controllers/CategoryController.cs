using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

   
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

       
        public async Task<IActionResult> Index(string Slug = "",string sort_by = "", string startprice = "", string endprice = "")
        {
         
            if (string.IsNullOrWhiteSpace(Slug))
            {
                return RedirectToAction("Index"); 
            }

        
            CategoryModel category = await _dataContext.Categories
                .Where(c => c.Slug == Slug)
                .FirstOrDefaultAsync();

           
            if (category == null)
            {
                return RedirectToAction("Index"); 
            }


            IQueryable<ProductModel> productsByCategory = _dataContext.Products
                .Where(p => p.CategoryID == category.Id);
            var count = await productsByCategory.CountAsync();
            if (count > 0)
            {
               
                if (sort_by == "price_increase")
                {
                    productsByCategory = productsByCategory.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    productsByCategory = productsByCategory.OrderBy(p => p.Id);
                }
                //lọc giá
                else if (startprice != "" && endprice != "")
                {
                    decimal startPriceValue;
                    decimal endPriceValue;

                    if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                    {
                        productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                }
            }

                var productsList = await productsByCategory
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(await productsByCategory.ToListAsync());
        }
    }
}
