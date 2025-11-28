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

        // --- CỘT MỚI: LIÊN KẾT VỚI TÀI KHOẢN ---
        public int? TaiKhoanId { get; set; } // Cho phép null (vì khách vãng lai cũng gửi được)

        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }
        // ---------------------------------------

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