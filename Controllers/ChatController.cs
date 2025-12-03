using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;
using System.Collections.Generic;
using System;

namespace banthietbidientu.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. TRANG DASHBOARD (DÀNH CHO ADMIN/BOSS)
        // =========================================================
        [Authorize(Roles = "Admin,Boss")]
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================
        // 2. API LẤY DANH SÁCH NGƯỜI CHAT (CHO SIDEBAR ADMIN)
        // =========================================================
        [Authorize(Roles = "Admin,Boss")]
        [HttpGet]
        public IActionResult GetContacts()
        {
            var currentUser = User.Identity.Name;

            // 1. Lấy tất cả tin nhắn LIÊN QUAN ĐẾN MÌNH (Gửi đi hoặc Nhận về)
            var messages = _context.TinNhans.AsNoTracking()
                .Where(m => m.SenderName == currentUser || m.ReceiverName == currentUser)
                .ToList();

            // 2. Lấy danh sách những người đã chat với mình
            // (Loại bỏ user "Admin" hệ thống nếu có)
            var users = messages
                .Select(m => m.SenderName == currentUser ? m.ReceiverName : m.SenderName)
                .Distinct()
                .Where(u => !string.IsNullOrEmpty(u) && u != "Admin")
                .ToList();

            // 3. Thêm Boss và Admin khác vào danh sách (Chat nội bộ)
            var internalUsers = _context.TaiKhoans
                .Where(u => u.Username != currentUser && (u.Role == "Boss" || u.Role == "Admin"))
                .Select(u => u.Username)
                .ToList();

            users.AddRange(internalUsers);
            users = users.Distinct().ToList();

            var contactList = new List<object>();

            foreach (var u in users)
            {
                // Lấy tin cuối cùng giữa 2 người
                var lastMsg = messages
                    .Where(m => (m.SenderName == u && m.ReceiverName == currentUser) ||
                                (m.SenderName == currentUser && m.ReceiverName == u))
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault();

                // Đếm tin chưa đọc
                int unread = messages.Count(m => m.SenderName == u && m.ReceiverName == currentUser && !m.IsRead);

                // Lấy thông tin tài khoản
                var account = _context.TaiKhoans.FirstOrDefault(a => a.Username == u);

                // Sắp xếp ưu tiên: Boss (1) > Admin (2) > Khách (3)
                int priority = 3;
                if (account?.Role == "Boss") priority = 1;
                else if (account?.Role == "Admin") priority = 2;

                // Hiển thị tên thật hoặc username (tránh null)
                string displayName = !string.IsNullOrEmpty(account?.FullName) ? account.FullName : (account?.Username ?? u);

                string displayTime = lastMsg?.Timestamp.ToString("HH:mm dd/MM") ?? "";

                contactList.Add(new
                {
                    username = u,
                    fullName = displayName,
                    lastMessage = lastMsg?.Content ?? "Bắt đầu trò chuyện",
                    time = displayTime,
                    unread = unread,
                    priority = priority,
                    avatar = account?.Role == "Boss" ? "/image/logo.png" : "/image/logo.png" // Ảnh đại diện (có thể thay đổi sau)
                });
            }

            // Sắp xếp danh sách: Theo Priority -> Theo thời gian tin mới nhất
            return Json(contactList.OrderBy(x => ((dynamic)x).priority).ThenByDescending(x => ((dynamic)x).time));
        }

        // =========================================================
        // 3. API LẤY LỊCH SỬ TIN NHẮN
        // =========================================================
        [HttpGet]
        public IActionResult GetHistory(string otherUser)
        {
            var currentUser = User.Identity.Name;
            var query = _context.TinNhans.AsQueryable();

            if (User.IsInRole("Admin") || User.IsInRole("Boss"))
            {
                // Admin xem tin với user cụ thể
                query = query.Where(m => (m.SenderName == currentUser && m.ReceiverName == otherUser) ||
                                         (m.SenderName == otherUser && m.ReceiverName == currentUser));

                // Đánh dấu đã đọc các tin người đó gửi cho mình
                var unreadMsgs = query.Where(m => m.ReceiverName == currentUser && !m.IsRead).ToList();
                if (unreadMsgs.Any())
                {
                    foreach (var m in unreadMsgs) m.IsRead = true;
                    _context.SaveChanges();
                }
            }
            else
            {
                // Khách xem tin của mình (với Admin hoặc hệ thống)
                query = query.Where(m => m.SenderName == currentUser || m.ReceiverName == currentUser);
            }

            var history = query.OrderBy(m => m.Timestamp)
                .Select(m => new {
                    sender = m.SenderName,
                    content = m.Content,
                    time = m.Timestamp.ToString("HH:mm")
                })
                .ToList();

            return Json(history);
        }
    }
}