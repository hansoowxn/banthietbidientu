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
        public bool DaDoc { get; set; } = false; // false = chưa đọc (hiện số đỏ)
        public int LoaiThongBao { get; set; } // 0: Đơn mới, 1: Đã giao...
    }
}