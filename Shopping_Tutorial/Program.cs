using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.MoMo;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);


//Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();
//Khai báo momo API
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddHttpClient("API_Hub", client =>
{
    client.BaseAddress = new Uri("https://localhost:7092");
});

// Cấu hình DbContext với chuỗi kết nối từ appsettings.json
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"));
});

builder.Services.AddTransient<IEmailSender,EmailSender>();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



// Cấu hình Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>() // Sửa lại ở đây, thay DbContext bằng DataContext
    .AddDefaultTokenProviders();

// Cấu hình IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false; // Yêu cầu ký tự đặc biệt
    options.Password.RequireUppercase = false; // Yêu cầu chữ hoa
    options.Password.RequiredLength = 6; // Số lượng ký tự tối thiểu để đăng nhập

    // Các thiết lập về khóa tài khoản
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Thời gian khóa tài khoản
    //options.Lockout.MaxFailedAccessAttempts = 5; // Số lần thử đăng nhập sai
    //options.Lockout.AllowedForNewUsers = true;

    // Các ký tự cho phép trong tên người dùng
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
//ket noi GOOGLE
builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});




var app = builder.Build();

// Cấu hình trang lỗi
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");
app.UseSession(); // Cấu hình sử dụng session

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Cấu hình trang lỗi cho môi trường không phải phát triển
}
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".glb"] = "model/gltf-binary";
    provider.Mappings[".gltf"] = "model/gltf+json";

    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider
    });

app.UseRouting(); // Cấu hình cho việc định tuyến

app.UseAuthentication(); // Cấu hình xác thực
app.UseAuthorization(); // Cấu hình phân quyền


app.UseCors(); // Sử dụng CORS
// Các route cho các khu vực
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Product}/{action=Index}/{id?}");

// Các route cho category và brand
app.MapControllerRoute(
    name: "category",
    pattern: "/category/{Slug?}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new { controller = "Brand", action = "Index" });

// Route mặc định cho trang chủ
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Homepage}/{id?}");


// Khởi tạo dữ liệu khi ứng dụng bắt đầu
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);

// Chạy ứng dụng
app.Run();
