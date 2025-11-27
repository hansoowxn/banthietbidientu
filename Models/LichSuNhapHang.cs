using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    public class LichSuNhapHang
    {
        [Key]
        public int Id { get; set; }

        public int SanPhamId { get; set; }

        public string? TenSanPham { get; set; } // Lưu tên cứng để sau này SP có bị xóa vẫn biết nhập cái gì

        public int SoLuongNhap { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaNhap { get; set; }

        public DateTime NgayNhap { get; set; } = DateTime.Now;

        public string? GhiChu { get; set; }

        // Liên kết với bảng Sản Phẩm (để truy vấn ngược nếu cần)
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
    }
}