using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class BaoCaoViewModel
    {
        // --- 1. KPI CARDS ---
        public decimal TongDoanhThu { get; set; } // Doanh thu thực (đã trừ giảm giá)
        public decimal TongDoanhThuGoc { get; set; } // Doanh thu niêm yết (chưa trừ giảm giá) - Để so sánh
        public int TongDonHang { get; set; }
        public int TongSanPhamDaBan { get; set; }
        public int SanPhamSapHet { get; set; }
        public decimal LoiNhuanUocTinh { get; set; } // Lợi nhuận thực tế

        // --- 2. BIỂU ĐỒ ---
        public List<string> LabelsNgay { get; set; }
        public List<decimal> ValuesDoanhThu { get; set; }
        public List<decimal> DataNamHienTai { get; set; }
        public List<decimal> DataNamTruoc { get; set; }
        public List<decimal> DataNamKia { get; set; }
        public int CurrentYear { get; set; }

        // --- 3. TOP LIST ---
        public List<string> TopSanPhamTen { get; set; }
        public List<int> TopSanPhamSoLuong { get; set; }
        public List<TopKhachHang> TopKhachHangs { get; set; }

        // [MỚI] KHÁCH HÀNG TIỀM NĂNG (VIP USER)
        public List<KhachHangTiemNang> KhachHangTiemNangs { get; set; }

        // --- 4. FILTER & DETAILS ---
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

    // [MỚI] Class cho Khách tiềm năng
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

        public decimal DoanhThuNiemYet { get; set; } // Giá bán trên web * Số lượng
        public decimal DoanhThuThuc { get; set; }    // Sau khi phân bổ chiết khấu
        public decimal GiaVon { get; set; }
        public decimal LoiNhuan { get; set; }        // DoanhThuThuc - GiaVon

        public double Margin => DoanhThuThuc > 0 ? (double)(LoiNhuan / DoanhThuThuc) * 100 : 0;
    }
}