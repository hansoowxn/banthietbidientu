using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; } // Giá bán hiện tại

        // --- CỘT MỚI: GIÁ NHẬP (Giá vốn bình quân) ---
        // Dùng để tính lãi dự kiến khi xem tồn kho
        [Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; } = 0;

        public string Description { get; set; }

        public string Category { get; set; }

        public string ImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; } // Số lượng tồn kho

        public string MoTa { get; set; } // Mô tả chi tiết (có thể chứa HTML)

        // --- Navigation Properties (Liên kết bảng) ---
        public virtual ICollection<GioHang> GioHangs { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }

        // Liên kết với chi tiết phiếu nhập (để tra cứu lịch sử nhập)
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}