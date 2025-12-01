using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("Voucher")]
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(20, ErrorMessage = "Mã tối đa 20 ký tự")]
        public string MaVoucher { get; set; } // VD: TET2026

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        public string TenVoucher { get; set; } // VD: Lì xì đầu năm

        // 0: Giảm theo tiền mặt (VD: 50k), 1: Giảm theo % (VD: 10%)
        public int LoaiGiamGia { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public decimal GiaTri { get; set; } // Số tiền giảm hoặc Số % giảm

        // Dành cho loại %: Giảm tối đa bao nhiêu? (VD: 10% tối đa 500k)
        public decimal GiamToiDa { get; set; } = 0;

        // Điều kiện: Đơn tối thiểu (Nhập 0 nếu không có điều kiện)
        public decimal DonToiThieu { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } // Tổng số lượng phát hành
        public int DaDung { get; set; } = 0; // Số lượng đã dùng

        public DateTime NgayBatDau { get; set; } = DateTime.Now;
        public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddDays(7);

        public bool IsActive { get; set; } = true;
    }
}