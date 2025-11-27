using System;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class DonHangViewModel
    {
        // --- 1. THÔNG TIN CHUNG CỦA ĐƠN HÀNG (HEADER) ---
        public string MaDon { get; set; }
        public int IdGiaLap { get; set; }

        public DateTime? NgayDat { get; set; }
        public string TrangThai { get; set; }
        public decimal TongTien { get; set; }
        public int? TongSoLuong { get; set; }

        // --- 2. THÔNG TIN KHÁCH HÀNG & THANH TOÁN ---
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string PhuongThucThanhToan { get; set; }

        // --- 3. THÔNG TIN SẢN PHẨM (ITEM) ---
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }

        // [THÊM DÒNG NÀY ĐỂ SỬA LỖI]
        public string? PhanLoai { get; set; }

        public int SoLuong { get; set; }
        public decimal? Gia { get; set; }

        // --- 4. DANH SÁCH CHI TIẾT ---
        public List<DonHangViewModel> SanPhams { get; set; } = new List<DonHangViewModel>();
    }
}