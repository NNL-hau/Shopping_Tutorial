using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IProductRecommendationService _recommendationService;

        public RecommendationController(IProductRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm gợi ý dựa trên lịch sử tìm kiếm
        /// </summary>
        /// <param name="count">Số lượng sản phẩm cần lấy (mặc định: 6)</param>
        /// <returns>Danh sách sản phẩm gợi ý</returns>
        [HttpGet("products")]
        public async Task<IActionResult> GetRecommendedProducts([FromQuery] int count = 6)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Người dùng chưa đăng nhập",
                        data = new List<object>()
                    });
                }

                var products = await _recommendationService.GetRecommendedProductsAsync(userId, count);

                var result = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    image = p.Image,
                    categoryName = p.Category?.Name,
                    brandName = p.Brand?.Name,
                    quantity = p.Quantity,
                    sold = p.Sold
                });

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách sản phẩm gợi ý thành công",
                    count = products.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy sản phẩm gợi ý",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lưu lịch sử tìm kiếm của người dùng
        /// </summary>
        /// <param name="request">Thông tin tìm kiếm</param>
        /// <returns>Kết quả lưu lịch sử</returns>
        [HttpPost("search-history")]
        public async Task<IActionResult> SaveSearchHistory([FromBody] SearchHistoryRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Người dùng chưa đăng nhập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Từ khóa tìm kiếm không được để trống"
                    });
                }

                await _recommendationService.SaveSearchHistoryAsync(userId, request.SearchTerm);

                return Ok(new
                {
                    success = true,
                    message = "Lưu lịch sử tìm kiếm thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lưu lịch sử tìm kiếm",
                    error = ex.Message
                });
            }
        }
    }

    public class SearchHistoryRequest
    {
        public string SearchTerm { get; set; }
    }
}