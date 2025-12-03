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
            if (string.IsNullOrWhiteSpace(message)) return;
            if (string.IsNullOrEmpty(user)) user = "Khách ẩn danh";
            if (string.IsNullOrEmpty(receiver)) receiver = "Admin";

            try
            {
                // 1. Tự động tìm StoreId nếu thiếu (Dành cho Admin/Boss)
                if ((storeId == null || storeId == 0) && user != "Khách ẩn danh")
                {
                    var senderAccount = await _context.TaiKhoans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Username == user);

                    if (senderAccount != null)
                    {
                        storeId = senderAccount.StoreId;
                    }
                }

                // 2. Tạo tin nhắn
                var msg = new TinNhan
                {
                    SenderName = user,
                    ReceiverName = receiver,
                    Content = message.Trim(),
                    Timestamp = DateTime.Now,
                    StoreId = (storeId.HasValue && storeId.Value > 0) ? storeId.Value : null,
                    IsRead = false
                };

                _context.TinNhans.Add(msg);

                // 3. Lưu vào DB
                await _context.SaveChangesAsync();

                // 4. Gửi Realtime cho mọi người
                await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                Console.WriteLine($"[CHAT ERROR] {ex.Message}");
                // Báo lỗi về cho người gửi
                await Clients.Caller.SendAsync("ReceiveMessage", "Hệ thống", "Lỗi gửi tin: " + ex.Message, DateTime.Now.ToString("HH:mm"));
            }
        }
    }
}