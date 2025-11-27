using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    public class LichSuMuaHang
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Liên kết với tài khoản người dùng

        [Required]
        public int ProductId { get; set; } // Liên kết với sản phẩm

        [Required]
        public string Name { get; set; } // Tên sản phẩm

        public string? TrangThaiDonHang { get; set; }

        public string? TrangThai { get; set; } = "Chờ xử lý"; // Mặc định là Chờ xử lý

        public string ImageUrl { get; set; } // URL hình ảnh sản phẩm

        [Required]
        public decimal Price { get; set; } // Giá sản phẩm

        [Required]
        public int Quantity { get; set; } // Số lượng mua

        [Required]
        public DateTime PurchaseDate { get; set; } // Ngày mua

        [Required]
        public string PaymentMethod { get; set; } // Phương thức thanh toán

        

        [ForeignKey("UserId")]
        public virtual TaiKhoan TaiKhoan { get; set; } // Navigation property

        [ForeignKey("ProductId")]
        public virtual SanPham SanPham { get; set; } // Navigation property
    }
}