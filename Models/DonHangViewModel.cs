using System;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class DonHangViewModel
    {
        public string MaDon { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // Các trường chi tiết cho hóa đơn / giao hàng
        public string SoDienThoai { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string PhuongThucThanhToan { get; set; }

        // [QUAN TRỌNG] Thêm thuộc tính này để fix lỗi CS0117
        public int? StoreId { get; set; }

        // Danh sách sản phẩm con
        public List<DonHangViewModel> SanPhams { get; set; }

        // Thuộc tính chi tiết sản phẩm
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal? Gia { get; set; }
    }
}