using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        // Liên kết với Sản phẩm được đánh giá
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }

        // Liên kết với Người đánh giá
        public int TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        // Liên kết với Đơn hàng (Để xác thực "Đã mua hàng")
        public string MaDon { get; set; }
        [ForeignKey("MaDon")]
        public virtual DonHang DonHang { get; set; }

        [Range(1, 5)]
        public int Sao { get; set; } // Số sao đánh giá (1-5)

        [Required]
        public string NoiDung { get; set; } // Nội dung bình luận

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}