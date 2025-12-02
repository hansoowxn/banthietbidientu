using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    // Bảng cấu hình: Lưu mức lương hiện tại của từng nhân viên
    [Table("CauHinhLuong")]
    public class CauHinhLuong
    {
        [Key]
        public int Id { get; set; }

        public int TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public decimal LuongCung { get; set; } // VD: 5.000.000
        public double PhanTramHoaHong { get; set; } // VD: 1.5 (nghĩa là 1.5%)
    }

    // Bảng lịch sử: Lưu bảng lương đã chốt của từng tháng
    [Table("BangLuong")]
    public class BangLuong
    {
        [Key]
        public int Id { get; set; }

        public int TaiKhoanId { get; set; } // Nhân viên được nhận lương
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public int Thang { get; set; }
        public int Nam { get; set; }

        public decimal DoanhSoDatDuoc { get; set; } // Tổng tiền bán được trong tháng
        public decimal LuongCung { get; set; }
        public decimal TienHoaHong { get; set; } // = DoanhSo * %
        public decimal TongThucNhan { get; set; } // = LuongCung + TienHoaHong

        public DateTime NgayChot { get; set; } = DateTime.Now;
        public string NguoiChot { get; set; } // Username của Boss
    }
}