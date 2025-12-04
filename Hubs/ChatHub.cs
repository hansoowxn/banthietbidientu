using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;

namespace banthietbidientu.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string receiver, string message, int? storeId)
        {
            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(message)) return;
            if (string.IsNullOrEmpty(user)) user = "Khách ẩn danh";
            if (string.IsNullOrEmpty(receiver)) receiver = "Admin";

            try
            {
                // 1. Tìm thông tin người gửi để lấy StoreId và Role
                string senderRole = "User"; // Mặc định coi là Khách (User)

                var senderAccount = await _context.TaiKhoans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == user);

                if (senderAccount != null)
                {
                    // Lấy chức vụ thực tế (Admin/Boss) để Client xử lý thông báo
                    senderRole = senderAccount.Role;

                    // Tự động điền StoreId nếu Admin/Boss quên gửi hoặc gửi thiếu
                    if (storeId == null || storeId == 0)
                    {
                        storeId = senderAccount.StoreId;
                    }
                }

                // 2. Tạo đối tượng tin nhắn
                var msg = new TinNhan
                {
                    SenderName = user,
                    ReceiverName = receiver,
                    Content = message.Trim(),
                    Timestamp = DateTime.Now,
                    // Nếu StoreId vẫn null (ví dụ Boss chat), thì lưu null
                    StoreId = (storeId.HasValue && storeId.Value > 0) ? storeId.Value : null,
                    IsRead = false
                };

                _context.TinNhans.Add(msg);

                // 3. Lưu vào Database
                await _context.SaveChangesAsync();

                // 4. Gửi Realtime cho tất cả Client
                // [QUAN TRỌNG] Gửi kèm senderRole (tham số thứ 4) để Client lọc chuông
                await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"), senderRole);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi server
                Console.WriteLine($"[CHAT ERROR] {ex.Message}");

                // Báo lỗi về cho người gửi (kèm role System để client không bị lỗi)
                await Clients.Caller.SendAsync("ReceiveMessage", "Hệ thống", "Lỗi gửi tin: " + ex.Message, DateTime.Now.ToString("HH:mm"), "System");
            }
        }
    }
}