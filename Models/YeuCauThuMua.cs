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

        public string TenMay { get; set; }
        public string TinhTrang { get; set; }
        public string HinhAnh { get; set; } // Đường dẫn ảnh upload
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }

        // 0: Chờ xử lý, 1: Đã liên hệ, 2: Hoàn thành, -1: Hủy
        public int TrangThai { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}