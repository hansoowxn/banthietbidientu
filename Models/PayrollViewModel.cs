using System;

namespace banthietbidientu.Models
{
    public class PayrollViewModel
    {
        public int TaiKhoanId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public int? StoreId { get; set; } // Để hiện tên kho (Hà Nội/ĐN/HCM)

        public decimal LuongCung { get; set; }
        public double PhanTramHoaHong { get; set; }

        public decimal DoanhSo { get; set; } // Tổng bán được
        public decimal TienHoaHong { get; set; } // = DoanhSo * %
        public decimal TongThucNhan { get; set; } // = Lương cứng + Hoa hồng

        public bool DaChot { get; set; } // Đã trả lương tháng này chưa?
        public DateTime? NgayChot { get; set; }
    }
}