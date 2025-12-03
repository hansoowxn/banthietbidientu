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

        public string SenderName { get; set; } // Tên người gửi (Username)
        public string ReceiverName { get; set; } // Tên người nhận (Hoặc Group)

        public string Content { get; set; } // Nội dung tin nhắn

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false; // Trạng thái đã xem

        // Phân luồng Store để biết khách đang chat với khu vực nào
        // 1: Bắc, 2: Trung, 3: Nam. (Nếu là chat nội bộ thì có thể null hoặc quy định khác)
        public int? StoreId { get; set; }
    }
}