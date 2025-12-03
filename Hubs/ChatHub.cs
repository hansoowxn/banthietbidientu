using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
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

        // [QUAN TRỌNG] Phải có tham số 'string receiver' ở đây thì bên dưới mới dùng được
        public async Task SendMessage(string user, string receiver, string message, int? storeId)
        {
            // 1. Lưu vào Database
            var msg = new TinNhan
            {
                SenderName = user,
                ReceiverName = receiver, // Lưu tên người nhận
                Content = message,
                Timestamp = DateTime.Now,
                StoreId = storeId,
                IsRead = false
            };
            _context.TinNhans.Add(msg);
            await _context.SaveChangesAsync();

            // 2. Gửi lại cho TẤT CẢ mọi người (Clients.All)
            // Để client tự lọc xem tin nào là của mình
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"));
        }
    }
}