using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace banthietbidientu.Models
{
    public class YeuCauThuMuaViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên máy")]
        public string TenMay { get; set; } // VD: iPhone 13 Pro Max

        public string TinhTrang { get; set; } // VD: Mới 99%, Trầy xước...

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        public string? GhiChu { get; set; } // Khách muốn đổi sang máy gì, hoặc mô tả thêm

        [Display(Name = "Hình ảnh máy (nếu có)")]
        public IFormFile? HinhAnhMay { get; set; } // File ảnh upload lên
    }
}