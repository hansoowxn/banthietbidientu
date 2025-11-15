using System.ComponentModel.DataAnnotations;

namespace TestDoAn.Models
{
    public class TaiKhoan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
       
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters")]
        public string FullName { get; set; } // Họ và Tên

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } // Ngày sinh

        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        public string Address { get; set; } // Địa chỉ

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } // Giới tính

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; } // Email

        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
    }
}