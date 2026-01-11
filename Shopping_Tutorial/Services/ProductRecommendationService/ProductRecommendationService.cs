using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Services
{
    // Interface cho loose-coupling
    public interface IProductRecommendationService
    {
        Task<List<ProductModel>> GetRecommendedProductsAsync(string userId, int count = 6);
        Task SaveSearchHistoryAsync(string userId, string searchTerm);
    }

    public class ProductRecommendationService : IProductRecommendationService
    {
        private readonly DataContext _context;

        public ProductRecommendationService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ProductModel>> GetRecommendedProductsAsync(string userId, int count = 6)
        {
            // Lấy 3 lần tìm kiếm gần nhất
            var recentSearches = await _context.SearchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.SearchDate)
                .Take(3)
                .Select(h => h.SearchTerm)
                .ToListAsync();

            if (!recentSearches.Any())
            {
                // Nếu không có lịch sử, trả về sản phẩm bán chạy
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .OrderByDescending(p => p.Sold)
                    .Take(count)
                    .ToListAsync();
            }

            // Tìm sản phẩm liên quan đến các từ khóa tìm kiếm
            var recommendedProducts = new List<ProductModel>();

            foreach (var searchTerm in recentSearches)
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm) ||
                        p.Category.Name.Contains(searchTerm) ||
                        p.Brand.Name.Contains(searchTerm)
                    )
                    .Take(count)
                    .ToListAsync();

                recommendedProducts.AddRange(products);
            }

            // Loại bỏ trùng lặp và lấy số lượng cần thiết
            var uniqueProducts = recommendedProducts
                .GroupBy(p => p.Id)
                .Select(g => new
                {
                    Product = g.First(),
                    Relevance = g.Count() // Sản phẩm xuất hiện nhiều lần có độ liên quan cao hơn
                })
                .OrderByDescending(x => x.Relevance)
                .ThenByDescending(x => x.Product.Sold)
                .Take(count)
                .Select(x => x.Product)
                .ToList();

            // Nếu không đủ sản phẩm, bổ sung từ sản phẩm bán chạy
            if (uniqueProducts.Count < count)
            {
                var additionalProducts = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => !uniqueProducts.Select(up => up.Id).Contains(p.Id))
                    .OrderByDescending(p => p.Sold)
                    .Take(count - uniqueProducts.Count)
                    .ToListAsync();

                uniqueProducts.AddRange(additionalProducts);
            }

            return uniqueProducts;
        }

        public async Task SaveSearchHistoryAsync(string userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(searchTerm))
                return;

            var searchHistory = new SearchHistoryModel
            {
                UserId = userId,
                SearchTerm = searchTerm.Trim(),
                SearchDate = DateTime.Now
            };

            _context.SearchHistories.Add(searchHistory);
            await _context.SaveChangesAsync();
        }
    }
}