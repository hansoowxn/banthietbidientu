using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }

        // Liên kết với Đơn Hàng
        public string MaDon { get; set; }
        [ForeignKey("MaDon")]
        public virtual DonHang DonHang { get; set; }

        // Liên kết với Sản Phẩm
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }

        public int SoLuong { get; set; }

        public decimal? Gia { get; set; } // Giá bán ra cho khách (Doanh thu)

        // --- CỘT MỚI: GIÁ GỐC (Giá vốn lúc xuất kho) ---
        // Quan trọng để tính lợi nhuận thực tế: Lãi = (Gia - GiaGoc) * SoLuong
        public decimal GiaGoc { get; set; } = 0;
    }
}