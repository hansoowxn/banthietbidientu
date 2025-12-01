using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Services; // Đảm bảo namespace này có chứa EmailSender

var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CẤU HÌNH SESSION (QUAN TRỌNG CHO GIỎ HÀNG)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. ĐĂNG KÝ CÁC SERVICES
builder.Services.AddScoped<MemberService>();

// --- [FIX LỖI] ĐĂNG KÝ DỊCH VỤ EMAIL ---
// Dòng này bắt buộc phải có để AdminController và GioHangController hoạt động
builder.Services.AddTransient<IEmailSender, EmailSender>();
// ---------------------------------------

// 4. CẤU HÌNH MVC & JSON
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        // Giữ nguyên tên thuộc tính JSON (không tự đổi sang chữ thường)
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
    });

// 5. CẤU HÌNH ĐĂNG NHẬP (COOKIE)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/DangNhap";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

// --- CẤU HÌNH PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự quan trọng: Session -> Auth -> Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();