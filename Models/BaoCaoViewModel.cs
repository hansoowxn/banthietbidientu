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

        // 5. --- MỚI THÊM: DANH SÁCH LỢI NHUẬN SẢN PHẨM ---
        public List<LoiNhuanSanPham> BaoCaoLoiNhuan { get; set; }
    }

    public class TopKhachHang
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal TongChiTieu { get; set; }
        public int SoLanMua { get; set; }
    }

    // Class con chứa thông tin Lợi nhuận
    public class LoiNhuanSanPham
    {
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }     // Tổng số lượng bán
        public decimal DoanhThu { get; set; }   // Tổng tiền thu về (Giá bán * SL)
        public decimal GiaVon { get; set; }     // Tổng giá vốn (Giá gốc * SL)
        public decimal LoiNhuan { get; set; }   // DoanhThu - GiaVon
    }
}