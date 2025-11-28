using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "User";

        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // --- CHỈ ĐỂ 1 DÒNG NÀY THÔI ---
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}