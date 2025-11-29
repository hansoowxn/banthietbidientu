// hansoowxn/banthietbidientu/banthietbidientu-d71060d65eba5cf3c37443f90adfef16af5d09f1/Models/YeuCauThuMua.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("YeuCauThuMua")]
    public class YeuCauThuMua
    {
        [Key]
        public int Id { get; set; }

        // --- [MỚI] MÃ ĐƠN THU MUA (VD: YM000001) ---
        [StringLength(20)]
        public string MaYeuCau { get; set; }
        // ---------------------------------------------

        // --- CỘT LIÊN KẾT VỚI TÀI KHOẢN ĐÃ ĐĂNG NHẬP ---
        public int? TaiKhoanId { get; set; } // Cho phép null nếu bạn chưa áp dụng [Authorize]

        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }
        // ------------------------------------------------

        public string TenMay { get; set; }
        public string TinhTrang { get; set; }
        public string HinhAnh { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }

        // 0: Chờ xử lý, 1: Đã liên hệ, 2: Hoàn thành, -1: Hủy
        public int TrangThai { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}