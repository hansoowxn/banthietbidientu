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

        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }

        public int TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public string MaDon { get; set; }
        [ForeignKey("MaDon")]
        public virtual DonHang DonHang { get; set; }

        [Range(1, 5)]
        public int Sao { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // --- CÁC TRƯỜNG MỚI THÊM VÀO ---

        // 1. Lưu tên file ảnh (nếu khách có up ảnh review)
        public string HinhAnh { get; set; }

        // 2. Trạng thái duyệt (true = Hiện, false = Ẩn/Chờ duyệt)
        // Để mặc định là true (hiện ngay) cho tiện test, sau này muốn chặt chẽ thì sửa thành false
        public bool DaDuyet { get; set; } = true;

        // 3. Nội dung Admin trả lời lại đánh giá này
        public string TraLoi { get; set; }

        // 4. Ngày Admin trả lời
        public DateTime? NgayTraLoi { get; set; }
    }
}