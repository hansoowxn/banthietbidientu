using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("Banner")]
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình ảnh")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(100, ErrorMessage = "Tiêu đề không quá 100 ký tự")]
        public string Title { get; set; }

        [StringLength(255, ErrorMessage = "Mô tả không quá 255 ký tự")]
        public string Description { get; set; }

        // Link khi bấm vào nút (VD: /Home/ThuMuaMayCu)
        public string LinkUrl { get; set; } = "#products";

        // Chữ hiển thị trên nút (VD: Mua Ngay, Xem Chi Tiết)
        public string ButtonText { get; set; } = "Khám phá ngay";

        // Thứ tự hiển thị (1, 2, 3...)
        public int DisplayOrder { get; set; } = 0;

        // Trạng thái (Đang hiện hay ẩn)
        public bool IsActive { get; set; } = true;
    }
}