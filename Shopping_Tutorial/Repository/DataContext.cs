using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ContactModel> Contact { get; set; }

        public DbSet<WishlistModel> Wishlists { get; set; }

        public DbSet<CompareModel> Compares { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<MomoInforModel> MomoInfors { get; set; }
        public DbSet<VnpayModel> VnInfors { get; set; }
        public DbSet<UserCartItem> UserCartItems { get; set; }
        public DbSet<CompareHistoryModel> CompareHistories { get; set; }

        // 3D Models và Annotations
        public DbSet<Product3DModel> Product3DModels { get; set; }
        public DbSet<ProductAnnotationModel> ProductAnnotations { get; set; }
        public DbSet<ProductConfigurationModel> ProductConfigurations { get; set; }

        public DbSet<UserCoinModel> UserCoins { get; set; }
        public DbSet<GuessPriceHistoryModel> GuessPriceHistories { get; set; }
        public DbSet<LuckySpinHistoryModel> LuckySpinHistories { get; set; }
        public DbSet<DailyCheckinHistoryModel> DailyCheckinHistories { get; set; }
        // lịch sử tìm kiếm
        public DbSet<SearchHistoryModel> SearchHistories { get; set; }

    }
    
}
