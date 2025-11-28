using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool DaDoc { get; set; } = false;

        // 0: Đơn hàng mới, 1: Hệ thống, 2: Thu cũ đổi mới
        public int LoaiThongBao { get; set; }

        // --- CÁC TRƯỜNG MỚI THÊM (QUAN TRỌNG) ---
        // Lưu ID đích (Ví dụ: Mã đơn hàng "DH001" hoặc ID yêu cầu "5")
        public string RedirectId { get; set; }

        // Lưu tên Action cần chuyển tới (Ví dụ: "QuanLyDonHang" hoặc "QuanLyThuMua")
        public string RedirectAction { get; set; }
    }
}