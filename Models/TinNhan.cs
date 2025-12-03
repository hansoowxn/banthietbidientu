using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace banthietbidientu.Models
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        public int Id { get; set; }

        // Tên người gửi (Username hoặc "Khách...")
        [Required]
        public string SenderName { get; set; }

        // Người nhận ("Admin" hoặc Username cụ thể)
        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Quan trọng: Cho phép NULL để Boss hoặc tin hệ thống không bị lỗi
        public int? StoreId { get; set; }
    }
}