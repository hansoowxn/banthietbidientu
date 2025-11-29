using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        // [SỬA LẠI] Dùng MaDon làm Khóa Chính (String) để khớp với ChiTietDonHang
        [Key]
        public string MaDon { get; set; }

        // Bỏ cột Id (int) đi vì đã dùng MaDon làm Key rồi
        // public int Id { get; set; } 

        public int? TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public DateTime? NgayDat { get; set; }

        public decimal TongTien { get; set; }
        public decimal? PhiShip { get; set; }

        // 0: Chờ xử lý, 1: Đã xác nhận, 2: Đang giao, 3: Hoàn thành, -1: Hủy
        public int TrangThai { get; set; }

        public string NguoiNhan { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }

        // Cột StoreId (Quan trọng cho tính năng phân quyền)
        public int? StoreId { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}