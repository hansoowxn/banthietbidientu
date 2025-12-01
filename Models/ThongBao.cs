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
        public DateTime NgayTao { get; set; }
        public bool DaDoc { get; set; }

        // 0: Đơn hàng mới, 1: Đánh giá, 2: Thu cũ, 3: Hủy đơn
        public int LoaiThongBao { get; set; }

        public string RedirectAction { get; set; } // Action để chuyển hướng (VD: QuanLyDonHang)
        public string RedirectId { get; set; }     // ID của đối tượng (VD: Mã đơn hàng)

        // [MỚI] StoreId: Null = Thông báo chung (Boss), 1=HN, 2=ĐN, 3=HCM
        public int? StoreId { get; set; }
    }
}