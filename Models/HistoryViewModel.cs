using banthietbidientu.Models;
using System;
using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class HistoryViewModel
    {
        public string MaDon { get; set; } // <--- Quan trọng nhất: Mã đơn
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; } // Lưu chuỗi: "Đang giao", "Hoàn thành"
        public decimal TongTien { get; set; }
        public string PaymentMethod { get; set; }

        // Sửa dòng này: Dùng ChiTietDonHang thay vì LichSuMuaHang
        public List<ChiTietDonHang> SanPhams { get; set; }
    }
}