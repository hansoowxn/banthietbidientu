using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class BaoCaoViewModel
    {
        // --- 1. KPI CARDS ---
        public decimal TongDoanhThu { get; set; }
        public decimal TongDoanhThuGoc { get; set; }
        public int TongDonHang { get; set; }
        public int TongSanPhamDaBan { get; set; }
        public int SanPhamSapHet { get; set; }
        public decimal LoiNhuanUocTinh { get; set; }

        // [MỚI] CHỈ SỐ TĂNG TRƯỞNG (So với tháng trước)
        public double GrowthDoanhThu { get; set; }
        public double GrowthLoiNhuan { get; set; }
        public double GrowthDonHang { get; set; }
        public double GrowthSanPham { get; set; }

        // --- 2. BIỂU ĐỒ ---
        public List<string> LabelsNgay { get; set; }
        public List<decimal> ValuesDoanhThu { get; set; }
        public List<decimal> DataNamHienTai { get; set; }
        public List<decimal> DataNamTruoc { get; set; }
        public List<decimal> DataNamKia { get; set; }
        public int CurrentYear { get; set; }

        // [MỚI] BIỂU ĐỒ TRÒN (DANH MỤC)
        public List<string> CategoryLabels { get; set; }
        public List<decimal> CategoryValues { get; set; }

        // [MỚI] BIỂU ĐỒ SO SÁNH CHI NHÁNH (HN, ĐN, HCM)
        public List<decimal> StoreRevenueComparison { get; set; }

        // --- 3. TOP LIST & FILTER ---
        public List<string> TopSanPhamTen { get; set; }
        public List<int> TopSanPhamSoLuong { get; set; }
        public List<TopKhachHang> TopKhachHangs { get; set; }
        public List<KhachHangTiemNang> KhachHangTiemNangs { get; set; }

        public int? SelectedStoreId { get; set; }
        public string StoreName { get; set; }
        public List<LoiNhuanSanPham> BaoCaoLoiNhuan { get; set; }
    }

    public class TopKhachHang
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int SoLanMua { get; set; }
        public decimal TongChiTieu { get; set; }
    }

    public class KhachHangTiemNang
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string Username { get; set; }
        public string SoDienThoai { get; set; }
        public decimal TongChiTieu { get; set; }
        public int SoDonHang { get; set; }
    }

    public class LoiNhuanSanPham
    {
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThuNiemYet { get; set; }
        public decimal DoanhThuThuc { get; set; }
        public decimal GiaVon { get; set; }
        public decimal LoiNhuan { get; set; }
        public double Margin => DoanhThuThuc > 0 ? (double)(LoiNhuan / DoanhThuThuc) * 100 : 0;
    }
}