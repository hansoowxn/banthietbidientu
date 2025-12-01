using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("LichSuHoatDong")]
    public class LichSuHoatDong
    {
        [Key]
        public int Id { get; set; }

        public int? TaiKhoanId { get; set; } // Người thực hiện
        public string TenNguoiDung { get; set; } // Lưu cứng tên lúc đó (đề phòng xóa user sau này)

        public int? StoreId { get; set; } // Chi nhánh thực hiện (để Boss lọc xem chi nhánh nào làm gì)

        [Required]
        public string HanhDong { get; set; } // VD: Cập nhật đơn hàng, Xóa sản phẩm

        public string NoiDung { get; set; } // Chi tiết: Đổi từ X sang Y

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Liên kết (Optional)
        [ForeignKey("TaiKhoanId")]
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}