using Microsoft.EntityFrameworkCore;
using TestDoAn.Models;

namespace TestDoAn.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LichSuMuaHang> LichSuMuaHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình mối quan hệ GioHang - SanPham
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.SanPham)
                .WithMany(s => s.GioHangs)
                .HasForeignKey(g => g.ProductId);

            // Cấu hình mối quan hệ GioHang - TaiKhoan
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.TaiKhoan)
                .WithMany(t => t.GioHangs)
                .HasForeignKey(g => g.UserId);

            modelBuilder.Entity<GioHang>()
                .Property(g => g.UserId)
                .IsRequired(false);

            // Seeding dữ liệu mẫu cho SanPham
            modelBuilder.Entity<SanPham>().HasData(
                new SanPham { Id = 1, Name = "Smartphone Galaxy X", Price = 699.99m,SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Galaxy+X" },
                new SanPham { Id = 2, Name = "Laptop Pro 15", Price = 1299.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Laptop+Pro" },
                new SanPham { Id = 3, Name = "Wireless Headphones Elite", Price = 199.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Headphones" },
                new SanPham { Id = 4, Name = "Smartwatch Series 7", Price = 399.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Smartwatch" },
                new SanPham { Id = 5, Name = "4K Smart TV 55\"", Price = 799.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Smart+TV" },
                new SanPham { Id = 6, Name = "Bluetooth Speaker", Price = 149.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Speaker" },
                new SanPham { Id = 7, Name = "Gaming Console X", Price = 499.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Console" },
                new SanPham { Id = 8, Name = "Wireless Mouse", Price = 49.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Mouse" },
                new SanPham { Id = 9, Name = "Portable Charger 10000mAh", Price = 29.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Charger" },
                new SanPham { Id = 10, Name = "Tablet Air 10", Price = 499.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Tablet" },
                new SanPham { Id = 11, Name = "Wireless Keyboard", Price = 79.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Keyboard" },
                new SanPham { Id = 12, Name = "Action Camera 4K", Price = 249.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Camera" }
            );

            // Seeding dữ liệu mẫu cho TaiKhoan
            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    Id = 1,
                    Username = "admin",
                    Password = "password", // Lưu ý: Nên mã hóa mật khẩu trong thực tế
                    Role = "Admin",
                    FullName = "Quản Trị Viên",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Address = "123 Đường Admin, TP.HCM",
                    Gender = "Male",
                    Email = "admin@example.com"
                },
                new TaiKhoan
                {
                    Id = 2,
                    Username = "user",
                    Password = "password", // Lưu ý: Nên mã hóa mật khẩu trong thực tế
                    Role = "User",
                    FullName = "Nguyễn Văn A",
                    DateOfBirth = new DateTime(1995, 5, 15),
                    Address = "456 Đường User, Hà Nội",
                    Gender = "Male",
                    Email = "user@example.com"
                }
            );
        }
    }
}