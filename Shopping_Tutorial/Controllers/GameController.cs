using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;

namespace Shopping_Tutorial.Controllers
{
    [Route("api/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;

        public GameController(DataContext dataContext, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        private async Task EnsureMiniGameTablesAsync()
        {
            var createUserCoinsSql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserCoins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserCoins](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Coins] INT NOT NULL DEFAULT(0),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END";

            var createSpinHistorySql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LuckySpinHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LuckySpinHistories](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [PrizeType] NVARCHAR(50) NOT NULL,
        [PrizeValue] DECIMAL(18,6) NOT NULL,
        [CouponCode] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END";

            var createCheckinSql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DailyCheckinHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DailyCheckinHistories](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END";

            await _dataContext.Database.ExecuteSqlRawAsync(createUserCoinsSql);
            await _dataContext.Database.ExecuteSqlRawAsync(createSpinHistorySql);
            await _dataContext.Database.ExecuteSqlRawAsync(createCheckinSql);
        }

        [HttpGet("plays-left")]
        [Authorize]
        public async Task<IActionResult> GetPlaysLeft()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { success = false, message = "Vui lòng đăng nhập", playsLeft = 0 });
            }
            await EnsureMiniGameTablesAsync();
            var localStartUtc = DateTime.Now.Date.ToUniversalTime();
            var played = await _dataContext.LuckySpinHistories.CountAsync(x => x.UserId == user.Id && x.CreatedAt >= localStartUtc);
            var left = 5 - played;
            if (left < 0) left = 0;
            return Ok(new { success = true, playsLeft = left });
        }

        [HttpPost("spin-wheel")]
        [Authorize]
        public async Task<IActionResult> SpinWheel()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { success = false, message = "Vui lòng đăng nhập" });
            }
            await EnsureMiniGameTablesAsync();
            var localStartUtc = DateTime.Now.Date.ToUniversalTime();
            var playsToday = await _dataContext.LuckySpinHistories.CountAsync(x => x.UserId == user.Id && x.CreatedAt >= localStartUtc);
            if (playsToday >= 5)
            {
                return Ok(new { success = false, message = "Bạn đã dùng hết 5 lượt vòng quay hôm nay" });
            }

            var rnd = new Random();
            var prizes = new[]
            {
                new { type = "miss", value = 0m, label = "Chúc bạn may mắn", weight = 18 },
                new { type = "coin", value = 20m, label = "20 xu", weight = 24 },
                new { type = "voucher", value = 0.05m, label = "Giảm 5%", weight = 12 },
                new { type = "coin", value = 30m, label = "30 xu", weight = 20 },
                new { type = "voucher", value = 0.10m, label = "Giảm 10%", weight = 8 },
                new { type = "coin", value = 50m, label = "50 xu", weight = 16 },
                new { type = "voucher", value = 0.15m, label = "Giảm 15%", weight = 5 },
                new { type = "coin", value = 80m, label = "80 xu", weight = 10 },
                new { type = "voucher", value = 0.20m, label = "Giảm 20%", weight = 3 },
                new { type = "coin", value = 100m, label = "100 xu", weight = 8 },
                new { type = "voucher", value = 0.25m, label = "Giảm 25%", weight = 2 },
                new { type = "coin", value = 150m, label = "150 xu", weight = 4 }
            };
            var totalWeight = 0;
            foreach (var p in prizes) totalWeight += p.weight;
            var roll = rnd.Next(0, totalWeight);
            var acc = 0;
            var prize = prizes[0];
            foreach (var p in prizes)
            {
                acc += p.weight;
                if (roll < acc)
                {
                    prize = p;
                    break;
                }
            }

            string couponCode = null;
            string message;
            if (prize.type == "miss")
            {
                message = "Chúc bạn may mắn lần sau";
            }
            else if (prize.type == "coin")
            {
                var userCoin = await _dataContext.UserCoins.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (userCoin == null)
                {
                    userCoin = new UserCoinModel { UserId = user.Id, Coins = 0 };
                    _dataContext.UserCoins.Add(userCoin);
                }
                userCoin.Coins += (int)prize.value;
                userCoin.UpdatedAt = DateTime.UtcNow;
                await _dataContext.SaveChangesAsync();
                message = $"Bạn nhận được {prize.label}";
            }
            else
            {
                var code = $"WHEEL-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                var percent = Math.Round(prize.value, 2);
                var coupon = new CouponModel
                {
                    Name = code,
                    Description = $"Vòng quay may mắn giảm {percent * 100:0}% cho đơn hàng",
                    GiaNhap = percent,
                    DateStart = DateTime.Now,
                    DateExpired = DateTime.Now.AddDays(1),
                    Quantity = 1,
                    Status = 1
                };
                _dataContext.Coupons.Add(coupon);
                await _dataContext.SaveChangesAsync();
                couponCode = code;
                message = $"Bạn nhận được mã giảm giá {percent * 100:0}%";
            }

            var spin = new LuckySpinHistoryModel
            {
                UserId = user.Id,
                PrizeType = prize.type,
                PrizeValue = prize.value,
                CouponCode = couponCode
            };
            _dataContext.LuckySpinHistories.Add(spin);
            await _dataContext.SaveChangesAsync();

            var playsLeft = 5 - (playsToday + 1);

            return Ok(new
            {
                success = true,
                message = message,
                prizeType = prize.type,
                prizeValue = prize.value,
                couponCode = couponCode,
                playsLeft = playsLeft
            });
        }

        [HttpGet("checkin-status")]
        [Authorize]
        public async Task<IActionResult> CheckinStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { success = false, message = "Vui lòng đăng nhập", checkedIn = false });
            }
            await EnsureMiniGameTablesAsync();
            var localStartUtc = DateTime.Now.Date.ToUniversalTime();
            var todayCount = await _dataContext.DailyCheckinHistories.CountAsync(x => x.UserId == user.Id && x.CreatedAt >= localStartUtc);
            var checkedIn = todayCount > 0;
            return Ok(new { success = true, checkedIn });
        }

        [HttpPost("checkin")]
        [Authorize]
        public async Task<IActionResult> DailyCheckin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { success = false, message = "Vui lòng đăng nhập" });
            }
            await EnsureMiniGameTablesAsync();
            var localStartUtc = DateTime.Now.Date.ToUniversalTime();
            var todayCount = await _dataContext.DailyCheckinHistories.CountAsync(x => x.UserId == user.Id && x.CreatedAt >= localStartUtc);
            if (todayCount > 0)
            {
                return Ok(new { success = false, message = "Bạn đã điểm danh hôm nay rồi" });
            }
            var userCoin = await _dataContext.UserCoins.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (userCoin == null)
            {
                userCoin = new UserCoinModel { UserId = user.Id, Coins = 0 };
                _dataContext.UserCoins.Add(userCoin);
            }
            userCoin.Coins += 25;
            userCoin.UpdatedAt = DateTime.UtcNow;
            _dataContext.DailyCheckinHistories.Add(new DailyCheckinHistoryModel { UserId = user.Id });
            await _dataContext.SaveChangesAsync();
            return Ok(new { success = true, message = "Điểm danh thành công, nhận 25 xu", coins = userCoin.Coins });
        }
    }
}
