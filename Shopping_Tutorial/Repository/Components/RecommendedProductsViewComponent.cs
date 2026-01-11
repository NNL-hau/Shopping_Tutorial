using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Repository.Components
{
    public class RecommendedProductsViewComponent : ViewComponent
    {
        private readonly IProductRecommendationService _recommendationService;

        public RecommendedProductsViewComponent(IProductRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int take = 6)
        {
            if (!(UserClaimsPrincipal?.Identity?.IsAuthenticated ?? false))
            {
                return View("Default", new List<ProductModel>());
            }

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return View("Default", new List<ProductModel>());
            }

            var products = await _recommendationService.GetRecommendedProductsAsync(userId, take);

            return View("Default", products);
        }
    }
}