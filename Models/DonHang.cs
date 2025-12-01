using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        // Khóa chính là MaDon (String)
        [Key]
        public string MaDon { get; set; }

        public int? TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public DateTime? NgayDat { get; set; }

        public decimal TongTien { get; set; } // Tổng tiền khách phải trả (Đã gồm Thuế + Ship - Giảm giá)
        public decimal? PhiShip { get; set; }

        // [MỚI] Tiền thuế VAT (Lưu cứng giá trị tại thời điểm mua)
        public decimal TienThue { get; set; } = 0;

        public int TrangThai { get; set; } // 0: Mới, 1: Xác nhận, 2: Giao, 3: Xong, -1: Hủy

        public string NguoiNhan { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }

        public int? StoreId { get; set; } // Phân quyền admin chi nhánh

        // Thông tin Voucher (nếu có)
        public string? MaVoucher { get; set; }
        public decimal GiamGia { get; set; } = 0;

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}