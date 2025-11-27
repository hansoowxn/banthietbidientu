using System;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class BaoCaoViewModel
    {
        // 1. Các số liệu tổng quan
        public decimal TongDoanhThu { get; set; }
        public int TongSanPhamDaBan { get; set; }
        public int TongDonHang { get; set; }
        public int SanPhamSapHet { get; set; }

        // 2. Dữ liệu cho Biểu đồ Doanh thu (7 ngày qua)
        public List<string> LabelsNgay { get; set; }
        public List<decimal> ValuesDoanhThu { get; set; }

        // 3. Dữ liệu cho Biểu đồ Top Sản phẩm bán chạy
        public List<string> TopSanPhamTen { get; set; }
        public List<int> TopSanPhamSoLuong { get; set; }

        // 4. Danh sách Top khách hàng chi tiêu nhiều
        public List<TopKhachHang> TopKhachHangs { get; set; }
    }

    // Class con để chứa thông tin khách hàng VIP
    public class TopKhachHang
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // <--- THÊM DÒNG NÀY (Để phân biệt Admin)
        public decimal TongChiTieu { get; set; }
        public int SoLanMua { get; set; }
    }
}