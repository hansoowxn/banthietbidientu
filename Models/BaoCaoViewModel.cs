using System;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class BaoCaoViewModel
    {
        // 1. Các số liệu tổng quan
        public decimal TongDoanhThu { get; set; }
        public int TongSanPhamDaBan { get; set; }
        public int TongDonHang { get; set; } // Ước tính dựa trên lượt mua
        public int SanPhamSapHet { get; set; } // Cảnh báo kho

        // 2. Dữ liệu cho Biểu đồ Doanh thu (7 ngày qua)
        public List<string> LabelsNgay { get; set; } // ["12/11", "13/11"...]
        public List<decimal> ValuesDoanhThu { get; set; } // [1tr, 2tr...]

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
        public decimal TongChiTieu { get; set; }
        public int SoLanMua { get; set; }
    }
}