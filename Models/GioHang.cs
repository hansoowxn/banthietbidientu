namespace banthietbidientu.Models
{
    public class GioHang
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // Đổi từ string sang int?, nullable để hỗ trợ khách chưa đăng nhập
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public virtual SanPham SanPham { get; set; }
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}