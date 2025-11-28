using banthietbidientu.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace banthietbidientu.Data
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
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<LichSuNhapHang> LichSuNhapHangs { get; set; }
        public DbSet<DoanhThu> DoanhThus { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<YeuCauThuMua> YeuCauThuMuas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình mối quan hệ GioHang - SanPham
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.SanPham)
                .WithMany(s => s.GioHangs)
                .HasForeignKey(g => g.ProductId);

            modelBuilder.Entity<GioHang>()
                .Property(g => g.UserId)
                .IsRequired(false);

            // --- CẬP NHẬT DỮ LIỆU MẪU (Đã bổ sung Description và MoTa) ---
            modelBuilder.Entity<SanPham>().HasData(
                new SanPham
                {
                    Id = 1,
                    Name = "Smartphone Galaxy X",
                    Price = 699.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Galaxy+X",
                    Category = "Điện thoại",
                    GiaNhap = 500.00m,
                    Description = "Điện thoại thông minh cao cấp với màn hình tràn viền.",
                    MoTa = "Smartphone Galaxy X sở hữu thiết kế đột phá..."
                },
                new SanPham
                {
                    Id = 2,
                    Name = "Laptop Pro 15",
                    Price = 1299.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Laptop+Pro",
                    Category = "Laptop",
                    GiaNhap = 1000.00m,
                    Description = "Laptop hiệu năng cao cho công việc chuyên nghiệp.",
                    MoTa = "Laptop Pro 15 được trang bị chip xử lý mới nhất..."
                },
                new SanPham
                {
                    Id = 3,
                    Name = "Wireless Headphones Elite",
                    Price = 199.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Headphones",
                    Category = "Phụ kiện",
                    GiaNhap = 150.00m,
                    Description = "Tai nghe không dây chống ồn chủ động.",
                    MoTa = "Trải nghiệm âm thanh đỉnh cao với Wireless Headphones Elite..."
                },
                new SanPham
                {
                    Id = 4,
                    Name = "Smartwatch Series 7",
                    Price = 399.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Smartwatch",
                    Category = "Đồng hồ",
                    GiaNhap = 300.00m,
                    Description = "Đồng hồ thông minh theo dõi sức khỏe toàn diện.",
                    MoTa = "Smartwatch Series 7 hỗ trợ đo nhịp tim, SpO2..."
                },
                new SanPham
                {
                    Id = 5,
                    Name = "4K Smart TV 55\"",
                    Price = 799.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Smart+TV",
                    Category = "TV",
                    GiaNhap = 600.00m,
                    Description = "TV 4K sắc nét, trải nghiệm điện ảnh tại gia.",
                    MoTa = "Màn hình 55 inch độ phân giải 4K HDR..."
                },
                new SanPham
                {
                    Id = 6,
                    Name = "Bluetooth Speaker",
                    Price = 149.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Speaker",
                    Category = "Phụ kiện",
                    GiaNhap = 100.00m,
                    Description = "Loa Bluetooth di động âm bass mạnh mẽ.",
                    MoTa = "Bluetooth Speaker nhỏ gọn, pin trâu..."
                },
                new SanPham
                {
                    Id = 7,
                    Name = "Gaming Console X",
                    Price = 499.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Console",
                    Category = "Console",
                    GiaNhap = 400.00m,
                    Description = "Máy chơi game thế hệ mới, đồ họa đỉnh cao.",
                    MoTa = "Gaming Console X hỗ trợ chơi game 4K 120fps..."
                },
                new SanPham
                {
                    Id = 8,
                    Name = "Wireless Mouse",
                    Price = 49.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Mouse",
                    Category = "Phụ kiện",
                    GiaNhap = 30.00m,
                    Description = "Chuột không dây tiện lợi, độ nhạy cao.",
                    MoTa = "Thiết kế Ergonomic giúp cầm nắm thoải mái..."
                },
                new SanPham
                {
                    Id = 9,
                    Name = "Portable Charger 10000mAh",
                    Price = 29.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Charger",
                    Category = "Phụ kiện",
                    GiaNhap = 20.00m,
                    Description = "Sạc dự phòng dung lượng lớn, sạc nhanh.",
                    MoTa = "Dung lượng 10000mAh sạc được nhiều lần..."
                },
                new SanPham
                {
                    Id = 10,
                    Name = "Tablet Air 10",
                    Price = 499.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Tablet",
                    Category = "Tablet",
                    GiaNhap = 350.00m,
                    Description = "Máy tính bảng mỏng nhẹ, hiệu năng tốt.",
                    MoTa = "Màn hình Retina sắc nét, chip xử lý mạnh mẽ..."
                },
                new SanPham
                {
                    Id = 11,
                    Name = "Wireless Keyboard",
                    Price = 79.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Keyboard",
                    Category = "Phụ kiện",
                    GiaNhap = 50.00m,
                    Description = "Bàn phím không dây gõ êm, kết nối ổn định.",
                    MoTa = "Tương thích nhiều thiết bị, hành trình phím tốt..."
                },
                new SanPham
                {
                    Id = 12,
                    Name = "Action Camera 4K",
                    Price = 249.99m,
                    SoLuong = 1000,
                    ImageUrl = "https://via.placeholder.com/300x200?text=Camera",
                    Category = "Camera",
                    GiaNhap = 200.00m,
                    Description = "Camera hành trình quay 4K chống rung.",
                    MoTa = "Quay video 4K 60fps, chống nước..."
                }
            );

            // Seeding dữ liệu mẫu cho TaiKhoan
            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    Id = 1,
                    Username = "admin",
                    Password = "password",
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
                    Password = "password",
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