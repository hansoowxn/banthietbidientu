using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    public class PhieuNhap
    {
        [Key]
        public int Id { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public string GhiChu { get; set; } // Ví dụ: Nhập từ nhà cung cấp A

        // Tổng tiền nhập (Giá vốn)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public List<ChiTietPhieuNhap> ChiTiets { get; set; }
    }

    public class ChiTietPhieuNhap
    {
        [Key]
        public int Id { get; set; }
        public int PhieuNhapId { get; set; }
        [ForeignKey("PhieuNhapId")]
        public PhieuNhap PhieuNhap { get; set; }

        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public SanPham SanPham { get; set; }

        public int SoLuongNhap { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaNhap { get; set; } // Giá vốn nhập vào
    }
}