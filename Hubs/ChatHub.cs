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
                string senderRole = "User";
                var senderAccount = await _context.TaiKhoans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == user);

                if (senderAccount != null)
                {
                    senderRole = senderAccount.Role;
                    if (storeId == null || storeId == 0) storeId = senderAccount.StoreId;
                }

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
                await _context.SaveChangesAsync();

                // [QUAN TRỌNG] Thêm tham số thứ 5: storeId để Client lọc
                await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"), senderRole, storeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHAT ERROR] {ex.Message}");
                // Gửi lỗi (kèm storeId null)
                await Clients.Caller.SendAsync("ReceiveMessage", "Hệ thống", "Lỗi: " + ex.Message, DateTime.Now.ToString("HH:mm"), "System", null);
            }
        }
    }
}