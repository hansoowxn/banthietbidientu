namespace TestDoAn.Models
{
    public class SanPham
    {
        public int Id { get; set; }

        public int SoLuong { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string? Category { get; set; } // Nullable để tránh lỗi


        // Thuộc tính điều hướng: Một sản phẩm có thể liên kết với nhiều mục trong giỏ hàng
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
    }
}