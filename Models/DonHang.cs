using banthietbidientu.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    public class DonHang
    {
        [Key]
        public string MaDon { get; set; } // Hoặc int Id tùy thiết kế của bạn

        public DateTime? NgayDat { get; set; }
        public int TrangThai { get; set; } // 0: Chờ, 1: Duyệt, 2: Giao, 3: Xong, -1: Hủy
        public decimal TongTien { get; set; }
        public string NguoiNhan { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public decimal? PhiShip { get; set; }

        // Khóa ngoại liên kết với TaiKhoan (User)
        public int? TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        // Quan hệ 1-nhiều với ChiTietDonHang
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}