using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Shopping_Tutorial.Models;
using System.Numerics;

namespace Shopping_Tutorial.Controllers
{
    public class ChatController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly UserManager<AppUserModel> _userManager;

        public ChatController(DataContext context, IConfiguration configuration, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _userManager = userManager;
        }

        private sealed class AhpProductScore
        {
            public string ProductName { get; set; }
            public double OverallScore { get; set; }
            public double RevenueScore { get; set; }
            public double ProfitScore { get; set; }
            public double StockRiskScore { get; set; }
            public double SalesVelocityScore { get; set; }
        }

        private sealed class AhpResult
        {
            public double[] CriteriaWeights { get; set; }
            public List<AhpProductScore> TopRanked { get; set; }
        }

        // AHP tính nhanh: 4 tiêu chí, chuẩn hóa min-max, vector ưu tiên cố định (có thể điều chỉnh)
        private AhpResult ComputeAhpRanking(List<ProductModel> products)
        {
            // Bước 1: Xây dựng ma trận so sánh cặp cho tiêu chí (4 tiêu chí)
            double[,] pairwise = new double[,]
            {
        // Revenue, Profit, StockRisk, SalesVelocity
        { 1,   3,   5,   3 },   // Revenue so với các tiêu chí khác
        { 1.0/3, 1,   3,   2 }, // ProfitMargin so với các tiêu chí khác
        { 1.0/5, 1.0/3, 1,   0.5 }, // StockRisk
        { 1.0/3, 0.5, 2,   1 }  // SalesVelocity
            };

            int n = 4;
            double[] colSums = new double[n];
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                    colSums[j] += pairwise[i, j];
            }

            // Bước 2: Chuẩn hóa ma trận theo cột và tính trọng số (trung bình theo hàng)
            double[] weights = new double[n];
            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                    rowSum += pairwise[i, j] / colSums[j];
                weights[i] = rowSum / n;
            }

            // Bước 3: Kiểm tra tính nhất quán (CR)
            double[] lambdaVec = new double[n];
            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                    rowSum += pairwise[i, j] * weights[j];
                lambdaVec[i] = rowSum / weights[i];
            }
            double lambdaMax = lambdaVec.Average();
            double CI = (lambdaMax - n) / (n - 1);
            double[] RI = { 0.0, 0.0, 0.58, 0.90, 1.12 }; // Random Index cho n=1..5
            double CR = CI / RI[n];
            if (CR > 0.1)
            {
                // Nếu không nhất quán, có thể log cảnh báo hoặc trả về rỗng
                Console.WriteLine($"⚠️ Ma trận không nhất quán (CR={CR:P1})!");
            }

            if (products == null || products.Count == 0)
            {
                return new AhpResult { CriteriaWeights = weights, TopRanked = new List<AhpProductScore>() };
            }

            // Bước 4: Tính điểm chuẩn hóa cho từng sản phẩm (giống code cũ)
            var raw = products.Select(p => new
            {
                p.Name,
                Revenue = (double)(p.Price * p.Sold),
                ProfitMargin = (double)((p.Price > 0 ? (p.Price - p.CapitalPrice) / p.Price : 0)),
                StockRisk = (double)(p.Quantity - p.Sold),
                SalesVelocity = (double)p.Sold
            }).ToList();

            static (double min, double max) MinMax(IEnumerable<double> seq)
            {
                double min = double.PositiveInfinity, max = double.NegativeInfinity;
                foreach (var v in seq)
                {
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
                if (Math.Abs(max - min) < 1e-9) { max = min + 1; }
                return (min, max);
            }

            var (revMin, revMax) = MinMax(raw.Select(x => x.Revenue));
            var (pmMin, pmMax) = MinMax(raw.Select(x => x.ProfitMargin));
            var (srMin, srMax) = MinMax(raw.Select(x => x.StockRisk));
            var (svMin, svMax) = MinMax(raw.Select(x => x.SalesVelocity));

            double NormPos(double v, double min, double max) => (v - min) / (max - min);
            double NormNeg(double v, double min, double max) => 1.0 - (v - min) / (max - min);

            var ranked = raw.Select(x => new AhpProductScore
            {
                ProductName = x.Name,
                RevenueScore = NormPos(x.Revenue, revMin, revMax),
                ProfitScore = NormPos(x.ProfitMargin, pmMin, pmMax),
                StockRiskScore = NormNeg(x.StockRisk, srMin, srMax),
                SalesVelocityScore = NormPos(x.SalesVelocity, svMin, svMax)
            })
            .Select(s =>
            {
                s.OverallScore = s.RevenueScore * weights[0]
                                + s.ProfitScore * weights[1]
                                + s.StockRiskScore * weights[2]
                                + s.SalesVelocityScore * weights[3];
                return s;
            })
            .OrderByDescending(s => s.OverallScore)
            .Take(10)
            .ToList();

            return new AhpResult { CriteriaWeights = weights, TopRanked = ranked };
        }

        [HttpPost]
        public async Task<IActionResult> GetResponseCompare([FromBody] ChatRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                string latestComparison = "";

                if (user != null)
                {
                    var history = await _context.CompareHistories
                        .Where(h => h.UserId == user.Id)
                        .OrderByDescending(h => h.ComparisonDate)
                        .FirstOrDefaultAsync();

                    if (history != null)
                    {
                        latestComparison = $"Lịch sử so sánh gần đây nhất của tôi là các sản phẩm: {history.ComparedProductNames}.";
                    }
                }

                // Lấy thông tin sản phẩm từ database
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Ratings)
                    .ToListAsync();

                // Lấy thông tin mã giảm giá từ database (giống GetResponse)
                var coupons = await _context.Coupons.ToListAsync();
                var now = DateTime.Now;
                var activeCoupons = coupons
                    .Where(c => c.DateStart <= now && c.DateExpired >= now && c.Quantity > 0 && c.Status == 1)
                    .Select(c => new
                    {
                        code = c.Name,
                        description = c.Description,
                        startDate = c.DateStart.ToString("dd/MM/yyyy"),
                        endDate = c.DateExpired.ToString("dd/MM/yyyy"),
                        remaining = c.Quantity
                    })
                    .ToList();

                // Tạo context cho Gemini API
                var productContext = new
                {
                    products = products.Select(p => new
                    {
                        name = p.Name,
                        description = p.Description,
                        price = p.Price.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        category = p.Category?.Name,
                        brand = p.Brand?.Name,
                        ratings = p.Ratings?.Select(r => new { comment = r.Comment, star = r.Star }),
                        quantity = p.Quantity,
                        sold = p.Sold
                    }),
                    coupons = activeCoupons,
                };

                // Chuẩn bị prompt cho Gemini
                var introInfo = @"
   Bạn là 1 chat box hỗ trợ khách hàng lựa chọn sản phẩm.
    Không trả lời chung nếu chưa biết rõ khách hàng có ý như nào mà phải tư vấn cụ thệ.
    Hãy cho các phương án để người dùng có thể lựa chọn các sản phẩm tùy theo mong muốn của họ về các sẩn phẩm và thông số sản phẩm hỗ trợ họ trong các lĩnh vực khác nhau.
    Nếu khách muốn liên hệ trực tiếp để tư vấn thì gửi số chủ shop 0334626089-Nguyễn Quang Hùng còn không thì không nhắc gì đến
   Xưng em và tư vấn nhiệt tính , gửi sticker mỗi câu nói , không được trả lời mơ hồ mà phải trả lời 1 cách xúc tích, đưa phương án để khách chọn lựa, gợi ý các sản phẩm khác tốt hơn nếu có so với sản phẩm khách hàng quan tâm
    Có thể nêu ra các thông số mà khách hàng khác đánh giá nếu khách hỏi
    Hỗ trợ bảo hành sản phẩm trong vòng 1 năm nếu có lỗi từ nhà sản xuất
Mua có thể hỗ trợ từ MOMO , VNPAy và thanh toán trực ti
Khi khách hỏi về khuyến mãi/mã giảm giá, hãy sử dụng trường 'coupons' trong context để tư vấn các mã đang còn hiệu lực (code, mô tả, ngày hết hạn).
";

                var prompt = $@"
    Giới thiệu thêm:
    {introInfo}
    {latestComparison}

    Context: Đây là thông tin về các sản phẩm trong cửa hàng:
    {JsonSerializer.Serialize(productContext)}

    Câu hỏi của người dùng: {request.Message}

    Hãy trả lời câu hỏi dựa trên thông tin giới thiệu và sản phẩm trên. 
    Nếu câu hỏi không liên quan, hãy trả lời một cách thân thiện và hướng dẫn người dùng về các dịch vụ của cửa hàng.
";


                // Gọi Gemini API
                var apiKey = _configuration["Gemini:ApiKey"];
                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        }
                    }), System.Text.Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Không thể kết nối với Gemini API");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return Json(new { response = result?.candidates?[0]?.content?.parts?[0]?.text });
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetResponseAdmin([FromBody] ChatRequest request)
        {
            try
            {
                // Lấy thông tin sản phẩm từ database
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Ratings)
                    .ToListAsync();

                // Lấy dữ liệu thống kê
                var statistics = await _context.Statisticals
                    .OrderByDescending(s => s.DateCreated)
                    .Take(30) // Lấy 30 bản ghi gần nhất
                    .ToListAsync();

                // Lấy dữ liệu đơn hàng (tất cả đơn hàng)
                var orders = await _context.Orders
                    .OrderByDescending(o => o.CreatedDate)
                    .ToListAsync();

                // Lấy chi tiết đơn hàng (tất cả chi tiết)
                var orderDetails = await _context.OrderDetails
                    .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                    .Include(od => od.Product)
                    .ThenInclude(p => p.Brand)
                    .ToListAsync();

                // Tính toán thống kê tổng quan
                var totalRevenue = statistics.Sum(s => s.Revenue);
                var totalProfit = statistics.Sum(s => s.Profit);
                var totalOrders = statistics.Sum(s => s.Sold);
                var totalQuantitySold = statistics.Sum(s => s.Quantity);

                // ĐÁNH GIÁ AHP cho sản phẩm (hỗ trợ quyết định cho Admin)
                var ahpRanking = ComputeAhpRanking(products);

                // Thống kê sản phẩm bán chạy
                var topSellingProducts = products
                    .OrderByDescending(p => p.Sold)
                    .Take(10)
                    .Select(p => new
                    {
                        name = p.Name,
                        sold = p.Sold,
                        quantity = p.Quantity,
                        price = p.Price,
                        revenue = p.Sold * p.Price
                    });

                // Thống kê đơn hàng theo trạng thái
                var orderStatusStats = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count(),
                        totalAmount = g.Sum(o => orderDetails
                            .Where(od => od.OrderCode == o.OrderCode)
                            .Sum(od => od.Price * od.Quantity))
                    });

                // Tạo context cho Gemini API
                var adminContext = new
                {
                    products = products.Select(p => new
                    {
                        name = p.Name,
                        description = p.Description,
                        price = p.Price.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        capitalPrice = p.CapitalPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        category = p.Category?.Name,
                        brand = p.Brand?.Name,
                        ratings = p.Ratings?.Select(r => new { comment = r.Comment, star = r.Star }),
                        quantity = p.Quantity,
                        sold = p.Sold,
                        profit = (p.Price - p.CapitalPrice) * p.Sold
                    }),
                    ahp = new
                    {
                        criteria = new[]
                        {
                            new { name = "RevenueImpact", weight = ahpRanking.CriteriaWeights[0] },
                            new { name = "ProfitMargin", weight = ahpRanking.CriteriaWeights[1] },
                            new { name = "StockRisk", weight = ahpRanking.CriteriaWeights[2] },
                            new { name = "SalesVelocity", weight = ahpRanking.CriteriaWeights[3] }
                        },
                        topRecommendations = ahpRanking.TopRanked.Select(r => new
                        {
                            name = r.ProductName,
                            overallScore = r.OverallScore.ToString("0.000"),
                            revenueScore = r.RevenueScore.ToString("0.000"),
                            profitScore = r.ProfitScore.ToString("0.000"),
                            stockRiskScore = r.StockRiskScore.ToString("0.000"),
                            salesVelocityScore = r.SalesVelocityScore.ToString("0.000")
                        })
                    },
                    statistics = new
                    {
                        totalRevenue = totalRevenue.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        totalProfit = totalProfit.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        totalOrders = totalOrders,
                        totalQuantitySold = totalQuantitySold,
                        recentStats = statistics.Take(7).Select(s => new
                        {
                            date = s.DateCreated.ToString("dd/MM/yyyy"),
                            revenue = s.Revenue.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            profit = s.Profit.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            orders = s.Sold,
                            quantity = s.Quantity
                        })
                    },
                    orders = new
                    {
                        allOrders = orders.Select(o => new
                        {
                            orderCode = o.OrderCode,
                            userName = o.UserName,
                            createdDate = o.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                            status = o.Status,
                            paymentMethod = o.PaymentMethod,
                            shippingCost = o.ShippingCost.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            couponCode = o.CouponCode,
                            totalAmount = orderDetails.Where(od => od.OrderCode == o.OrderCode).Sum(od => od.Price * od.Quantity)
                        }),
                        recentOrders = orders.Take(10).Select(o => new
                        {
                            orderCode = o.OrderCode,
                            userName = o.UserName,
                            createdDate = o.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                            status = o.Status,
                            paymentMethod = o.PaymentMethod,
                            shippingCost = o.ShippingCost.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            couponCode = o.CouponCode
                        }),
                        orderStatusStats = orderStatusStats
                    },
                    orderDetails = new
                    {
                        allOrderDetails = orderDetails.Select(od => new
                        {
                            orderCode = od.OrderCode,
                            productName = od.Product?.Name,
                            productCategory = od.Product?.Category?.Name,
                            productBrand = od.Product?.Brand?.Name,
                            price = od.Price.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            quantity = od.Quantity,
                            totalAmount = (od.Price * od.Quantity).ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            userName = od.UserName
                        }),
                        orderDetailsByProduct = orderDetails.GroupBy(od => od.Product?.Name).Select(g => new
                        {
                            productName = g.Key,
                            totalQuantity = g.Sum(od => od.Quantity),
                            totalRevenue = g.Sum(od => od.Price * od.Quantity).ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                            orderCount = g.Select(od => od.OrderCode).Distinct().Count()
                        }).OrderByDescending(x => x.totalQuantity)
                    },
                    topSellingProducts = topSellingProducts
                };

                // Chuẩn bị prompt cho Gemini
                var introInfo = @"
Thình thoảng thêm icon để trả lời tốt hơn, Admin hỏi cụ thể gì thì trả lời, không thì hãy   . 
Chỉ trả lời bằng tiếng Việt và không trả lời Json
Bạn là một chatbot quản lý sản phẩm dành cho Admin, có khả năng đọc và phân tích dữ liệu từ các đơn hàng, sản phẩm, thống kê và đánh giá của khách hàng trong cơ sở dữ liệu để đề xuất các phương án quản lý hợp lý. Khi Admin yêu cầu, bạn sẽ thực hiện các công việc sau:

PHÂN TÍCH DỮ LIỆU THỐNG KÊ:
- Phân tích doanh thu, lợi nhuận và xu hướng bán hàng
- Đưa ra nhận xét về hiệu suất kinh doanh dựa trên dữ liệu thống kê
- So sánh hiệu suất giữa các thời kỳ khác nhau

PHÂN TÍCH ĐƠN HÀNG:
- Phân tích trạng thái đơn hàng (đã giao, đang xử lý, hủy bỏ)
- Đánh giá hiệu quả thanh toán (Momo, VNPay, COD)
- Phân tích việc sử dụng mã giảm giá và tác động đến doanh thu
- Phân tích chi tiết từng đơn hàng với thông tin sản phẩm, số lượng, giá cả, giá vốn
- Thống kê sản phẩm được mua nhiều nhất theo từng đơn hàng
- Phân tích hành vi mua hàng của khách hàng theo thời gian

ĐỀ XUẤT QUẢN LÝ GIÁ:
- Dựa trên dữ liệu bán hàng và lợi nhuận, đề xuất tăng/giảm giá sản phẩm
- Phân tích sản phẩm có lợi nhuận cao/thấp để điều chỉnh giá
- Đề xuất chiến lược giá cho sản phẩm bán chạy/kém

ĐỀ XUẤT QUẢN LÝ KHO:
- Dựa trên số lượng bán và tồn kho, đề xuất nhập/xuất hàng
- Cảnh báo sản phẩm sắp hết hàng hoặc tồn kho quá nhiều
- Đề xuất sản phẩm cần nhập thêm dựa trên xu hướng bán hàng

PHÂN TÍCH SẢN PHẨM:
- Đánh giá sản phẩm bán chạy nhất và ít bán nhất
- Phân tích đánh giá khách hàng để cải tiến sản phẩm
- Đề xuất sản phẩm cần cải tiến hoặc thay thế

BÁO CÁO TỔNG QUAN:
- Tổng hợp tình hình kinh doanh hiện tại
- Đưa ra các khuyến nghị chiến lược dựa trên dữ liệu
- Cảnh báo các vấn đề cần chú ý

Luôn đưa ra các phương án cụ thể với số liệu minh chứng và để Admin quyết định cuối cùng.

BỔ SUNG: Bạn có sẵn kết quả AHP trong trường 'ahp' của dữ liệu ngữ cảnh, với trọng số tiêu chí và top khuyến nghị đã chấm điểm. Hãy sử dụng AHP này để xếp hạng ưu tiên hành động (ví dụ: điều chỉnh giá, nhập hàng, đẩy quảng cáo) và nêu rõ lý do dựa trên các điểm thành phần (revenue, profit, stock risk, sales velocity). Không cần trình bày ma trận, chỉ nêu kết quả và khuyến nghị hành động tương ứng.

DỮ LIỆU CHI TIẾT:
- Trường 'orders.allOrders': Chứa tất cả đơn hàng với thông tin đầy đủ
- Trường 'orderDetails.allOrderDetails': Chứa tất cả chi tiết đơn hàng với thông tin sản phẩm
- Trường 'orderDetails.orderDetailsByProduct': Thống kê sản phẩm theo số lượng bán và doanh thu
- Sử dụng các trường này để phân tích sâu hơn về hành vi khách hàng và hiệu suất sản phẩm
";

                var prompt = $@"
    Giới thiệu: {introInfo}

    Dữ liệu hiện tại của cửa hàng:
    {JsonSerializer.Serialize(adminContext)}

    Câu hỏi/yêu cầu của Admin: {request.Message}

    Hãy phân tích dữ liệu và đưa ra các đề xuất quản lý cụ thể dựa trên thông tin trên.
";


                // Gọi Gemini API
                var apiKey = _configuration["Gemini:ApiKey"];
                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        }
                    }), System.Text.Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Không thể kết nối với Gemini API");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return Json(new { response = result?.candidates?[0]?.content?.parts?[0]?.text });
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetResponse([FromBody] ChatRequest request)
        {
            try
            {
                // Lấy thông tin sản phẩm từ database
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Ratings)
                    .ToListAsync();


                // Lấy thông tin mã giảm giá từ database
                var coupons = await _context.Coupons.ToListAsync();
                var now = DateTime.Now;
                var activeCoupons = coupons
                    .Where(c => c.DateStart <= now && c.DateExpired >= now && c.Quantity > 0 && c.Status == 1)
                    .Select(c => new
                    {
                        code = c.Name,
                        description = c.Description,
                        startDate = c.DateStart.ToString("dd/MM/yyyy"),
                        endDate = c.DateExpired.ToString("dd/MM/yyyy"),
                        remaining = c.Quantity
                    })
                    .ToList();

                // Lấy thông tin phí vận chuyển từ database
                var shippings = await _context.Shippings.ToListAsync();
                var shippingInfo = shippings.Select(s => new
                {
                    city = s.City,
                    district = s.District,
                    ward = s.Ward,
                    price = s.Price.ToString("N0", new System.Globalization.CultureInfo("vi-VN"))
                }).ToList();

                // Tạo context cho Gemini API
                var productContext = new
                {
                    products = products.Select(p => new
                    {
                        name = p.Name,
                        description = p.Description,
                        price = p.Price.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
                        category = p.Category?.Name,
                        brand = p.Brand?.Name,
                        ratings = p.Ratings?.Select(r => new { comment = r.Comment, star = r.Star }),
                        quantity = p.Quantity,
                        sold = p.Sold
                    }),
                    coupons = activeCoupons,
                    shipping = shippingInfo
                };

                // Chuẩn bị prompt cho Gemini
                var introInfo = @"
    Web và sản phẩm này được tạo lập bởi nhóm nghiên cứu khoa học do sinh viên Nguyễn Quang Bảo Hùng dẫn đầu.
    Cửa hàng của web có trụ sở tại Trường Đại học Kiến Trúc Hà Nội.
    Cách mua hàng online là thêm sản phẩm vào giỏ hàng (có tính phí vận chuyển theo khu vực).
    Có thể áp dụng mã giảm giá nếu có, các mã đặc biệt được phát miễn phí qua fanpage Facebook.Cách đăng nhập có thể đăng nhập thì có thể đăng kí trực tiếp và qua google, quên mật khẩu qua google và email
Liên hệ số 0334626089 hoặc email: anhhung9hot@gmail.com để được hỗ trợ từ nhân viên trực tiếp
    Hỗ trợ thanh toán qua momo và vnPay
   Khách hàng hỏi gì nếu chưa có dữ liệu thì ko đc nói là không có dữ liệu mà phải trả lời thực tế dựa trên thông tin sản phẩm khách hàng đang mua sắm và mô tả sản phẩm để dự đoán ra tương lai nhu cầu mua sắm
    Hỗ trợ đổi trả trong vòng 7 ngày nếu sản phẩm bị lỗi hoặc không đúng với mô tả 
    Hỗ trợ bảo hành sản phẩm trong vòng 1 năm nếu có lỗi từ nhà sản xuất
Muốn đánh giá sản phẩm thì bấm vào chi tiết sản phẩm và đánh giá theo mục 5 sao bằng cách gửi tên, email và nội dung đánh giá, chọn số sao
    Khi khách hỏi về khuyến mãi/mã giảm giá, hãy sử dụng trường 'coupons' trong context để tư vấn các mã đang còn hiệu lực (code, mô tả, ngày hết hạn).
    Khi khách hỏi về phí vận chuyển, hãy sử dụng trường 'shipping' trong context để tư vấn phí vận chuyển theo từng khu vực (thành phố, quận/huyện, phường/xã).
KHi khách hàng phân vân so sánh các sản phẩm thì chuyển hướng khách đến vào phần Tài khoản -> So sánh rồi bảo khách chọn các sản phẩm đã thêm vào so sánh để Hệ thống bên đấy hỗ trợ chọn
";

                var prompt = $@"
    Giới thiệu thêm:
    {introInfo}

    Context: Đây là thông tin về các sản phẩm trong cửa hàng:
    {JsonSerializer.Serialize(productContext)}

    Câu hỏi của người dùng: {request.Message}

    Hãy trả lời câu hỏi dựa trên thông tin giới thiệu và sản phẩm trên. 
    Nếu câu hỏi không liên quan, hãy trả lời một cách thân thiện và hướng dẫn người dùng về các dịch vụ của cửa hàng.
";


                // Gọi Gemini API
                var apiKey = _configuration["Gemini:ApiKey"];
                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        }
                    }), System.Text.Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Không thể kết nối với Gemini API");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return Json(new { response = result?.candidates?[0]?.content?.parts?[0]?.text });
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }

    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}