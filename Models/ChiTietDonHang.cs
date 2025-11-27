using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using banthietbidientu.Models;

namespace banthietbidientu.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }
        public int SoLuong { get; set; }
        public decimal? Gia { get; set; }

        // Liên kết bảng cha (Đơn hàng)
        public string MaDon { get; set; }
        [ForeignKey("MaDon")]
        public virtual DonHang DonHang { get; set; }

        // Liên kết bảng sản phẩm
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }
    }
}