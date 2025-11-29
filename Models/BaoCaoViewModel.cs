using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class BaoCaoViewModel
    {
        // --- 1. KPI CARDS (Các chỉ số chính) ---
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int TongSanPhamDaBan { get; set; } // Tên đúng khớp với Controller
        public int SanPhamSapHet { get; set; }    // Số lượng SP sắp hết
        public decimal LoiNhuanUocTinh { get; set; }

        // --- 2. BIỂU ĐỒ 7 NGÀY (Dữ liệu cũ - Vẫn giữ để không lỗi Controller) ---
        public List<string> LabelsNgay { get; set; }
        public List<decimal> ValuesDoanhThu { get; set; }

        // --- 3. BIỂU ĐỒ SO SÁNH 3 NĂM (Dữ liệu mới) ---
        public List<decimal> DataNamHienTai { get; set; }
        public List<decimal> DataNamTruoc { get; set; }
        public List<decimal> DataNamKia { get; set; }
        public int CurrentYear { get; set; }

        // --- 4. TOP SẢN PHẨM & KHÁCH HÀNG ---
        public List<string> TopSanPhamTen { get; set; }
        public List<int> TopSanPhamSoLuong { get; set; }
        public List<TopKhachHang> TopKhachHangs { get; set; }

        // --- 5. THÔNG TIN BỘ LỌC ---
        public int? SelectedStoreId { get; set; }
        public string StoreName { get; set; }

        // --- 6. BẢNG CHI TIẾT HIỆU QUẢ SẢN PHẨM ---
        public List<LoiNhuanSanPham> BaoCaoLoiNhuan { get; set; }
    }

    // Class hỗ trợ cho Top Khách Hàng
    public class TopKhachHang
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int SoLanMua { get; set; }
        public decimal TongChiTieu { get; set; }
    }

    // Class hỗ trợ cho Bảng Lợi Nhuận
    public class LoiNhuanSanPham
    {
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal GiaVon { get; set; }
        public decimal LoiNhuan { get; set; }

        // Tính % Biên lợi nhuận (Profit Margin)
        public double Margin => DoanhThu > 0 ? (double)(LoiNhuan / DoanhThu) * 100 : 0;
    }
}