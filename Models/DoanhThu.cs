using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    public class DoanhThu
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongDoanhThu { get; set; } // Tiền bán được

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongChiPhi { get; set; }   // Tiền nhập hàng (Vốn)

        [Column(TypeName = "decimal(18,2)")]
        public decimal LoiNhuan { get; set; }     // Thu - Chi
    }
}