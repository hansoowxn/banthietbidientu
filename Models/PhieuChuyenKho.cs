using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("PhieuChuyenKho")]
    public class PhieuChuyenKho
    {
        [Key]
        public int Id { get; set; }

        public string? MaPhieu { get; set; } // VD: CK20231201001

        public int TuKhoId { get; set; } // 1: HN, 2: DN, 3: SG
        public int DenKhoId { get; set; }

        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        public int SoLuong { get; set; }

        public string? NguoiTao { get; set; } // Username người tạo phiếu
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayHoanThanh { get; set; }

        // 0: Chờ xuất kho, 1: Đang vận chuyển, 2: Đã nhập kho, -1: Hủy
        public int TrangThai { get; set; } = 0;

        public string? GhiChu { get; set; }
    }
}