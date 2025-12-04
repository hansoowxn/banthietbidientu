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
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<LichSuHoatDong> LichSuHoatDongs { get; set; }
        public DbSet<PhieuChuyenKho> PhieuChuyenKhos { get; set; }
        public DbSet<CauHinhLuong> CauHinhLuongs { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }
        public DbSet<KhoHang> KhoHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình mối quan hệ GioHang - SanPham
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.SanPham)
                .WithMany(s => s.GioHangs)
                .HasForeignKey(g => g.ProductId);

            modelBuilder.Entity<GioHang>()
                .Property(g => g.UserId)
                .IsRequired(false);

            // 2. [CẬP NHẬT] Cấu hình Đơn Hàng (Dùng MaDon làm Khóa Chính)
            modelBuilder.Entity<DonHang>()
                .HasKey(d => d.MaDon); // Xác nhận MaDon là Khóa Chính (Primary Key)

            // Cấu hình mối quan hệ ChiTietDonHang - DonHang
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaDon); // Link qua MaDon

            // Cấu hình mối quan hệ DanhGia - DonHang
            modelBuilder.Entity<DanhGia>()
                .HasOne(dg => dg.DonHang)
                .WithMany() // DonHang không cần list DanhGia ngược lại
                .HasForeignKey(dg => dg.MaDon); // Link qua MaDon

            // --- SEED DATA (Dữ liệu mẫu) ---
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
                new SanPham { Id = 3, Name = "Wireless Headphones Elite", Price = 199.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Headphones", Category = "Phụ kiện", GiaNhap = 150.00m, Description = "Tai nghe không dây chống ồn chủ động.", MoTa = "Mô tả..." },
                new SanPham { Id = 4, Name = "Smartwatch Series 7", Price = 399.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Smartwatch", Category = "Đồng hồ", GiaNhap = 300.00m, Description = "Đồng hồ thông minh.", MoTa = "Mô tả..." },
                new SanPham { Id = 5, Name = "4K Smart TV 55\"", Price = 799.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Smart+TV", Category = "TV", GiaNhap = 600.00m, Description = "TV 4K sắc nét.", MoTa = "Mô tả..." },
                new SanPham { Id = 6, Name = "Bluetooth Speaker", Price = 149.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Speaker", Category = "Phụ kiện", GiaNhap = 100.00m, Description = "Loa Bluetooth.", MoTa = "Mô tả..." },
                new SanPham { Id = 7, Name = "Gaming Console X", Price = 499.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Console", Category = "Console", GiaNhap = 400.00m, Description = "Máy chơi game.", MoTa = "Mô tả..." },
                new SanPham { Id = 8, Name = "Wireless Mouse", Price = 49.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Mouse", Category = "Phụ kiện", GiaNhap = 30.00m, Description = "Chuột không dây.", MoTa = "Mô tả..." },
                new SanPham { Id = 9, Name = "Portable Charger 10000mAh", Price = 29.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Charger", Category = "Phụ kiện", GiaNhap = 20.00m, Description = "Sạc dự phòng.", MoTa = "Mô tả..." },
                new SanPham { Id = 10, Name = "Tablet Air 10", Price = 499.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Tablet", Category = "Tablet", GiaNhap = 350.00m, Description = "Máy tính bảng.", MoTa = "Mô tả..." },
                new SanPham { Id = 11, Name = "Wireless Keyboard", Price = 79.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Keyboard", Category = "Phụ kiện", GiaNhap = 50.00m, Description = "Bàn phím.", MoTa = "Mô tả..." },
                new SanPham { Id = 12, Name = "Action Camera 4K", Price = 249.99m, SoLuong = 1000, ImageUrl = "https://via.placeholder.com/300x200?text=Camera", Category = "Camera", GiaNhap = 200.00m, Description = "Camera hành trình.", MoTa = "Mô tả..." }
            );

            // SEED DATA TÀI KHOẢN
            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan { Id = 1, Username = "boss", Password = "123", Role = "Boss", StoreId = null, FullName = "Chủ Tịch (CEO)", Email = "boss@smarttech.com", PhoneNumber = "0999999999", Address = "Trụ sở chính", Gender = "Nam", DateOfBirth = new DateTime(1980, 1, 1) },
                new TaiKhoan { Id = 2, Username = "admin_hn", Password = "123", Role = "Admin", StoreId = 1, FullName = "Quản Lý Hà Nội", Email = "admin.hn@smarttech.com", PhoneNumber = "0988111222", Address = "Hà Nội", Gender = "Nam", DateOfBirth = new DateTime(1990, 1, 1) },
                new TaiKhoan { Id = 3, Username = "admin_dn", Password = "123", Role = "Admin", StoreId = 2, FullName = "Quản Lý Đà Nẵng", Email = "admin.dn@smarttech.com", PhoneNumber = "0988333444", Address = "Đà Nẵng", Gender = "Nữ", DateOfBirth = new DateTime(1992, 2, 2) },
                new TaiKhoan { Id = 4, Username = "admin_sg", Password = "123", Role = "Admin", StoreId = 3, FullName = "Quản Lý Sài Gòn", Email = "admin.sg@smarttech.com", PhoneNumber = "0988555666", Address = "TP.HCM", Gender = "Nam", DateOfBirth = new DateTime(1995, 3, 3) }
            );
        }
    }
}