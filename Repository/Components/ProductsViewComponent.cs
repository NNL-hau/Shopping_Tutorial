using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Tutorial.Repository.Components
{
    public class ProductsViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public ProductsViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? take)
        {
            var query = _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var products = await query.ToListAsync();
            return View(products);
        }
    }
}


