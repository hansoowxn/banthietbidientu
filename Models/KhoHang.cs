using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("KhoHang")]
    public class KhoHang
    {
        [Key]
        public int Id { get; set; }

        public int SanPhamId { get; set; }

        // 1: Hà Nội, 2: Đà Nẵng, 3: TP.HCM
        public int StoreId { get; set; }

        public int SoLuong { get; set; } = 0;

        // Liên kết ngược về sản phẩm
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }
    }
}