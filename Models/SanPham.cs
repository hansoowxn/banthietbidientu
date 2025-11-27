namespace banthietbidientu.Models
{
    public class SanPham
    {
        public int Id { get; set; }

        public int SoLuong { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? MoTa { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
    }
}