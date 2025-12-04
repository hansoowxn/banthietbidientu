using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        [Authorize(Roles = "Admin,Boss")]
        public IActionResult Index()
        {
            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
            ViewBag.CurrentStoreId = user?.StoreId ?? 0;
            ViewBag.IsBoss = user?.Role == "Boss";
            ViewBag.CurrentUserRole = user?.Role ?? "User";
            return View();
        }

        // --- 1. API GET CONTACTS (Đã bổ sung logic lấy Admin đồng nghiệp) ---
        [Authorize(Roles = "Admin,Boss")]
        [HttpGet]
        public IActionResult GetContacts(int? filterStoreId)
        {
            var currentUser = User.Identity.Name;
            var currentAdmin = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == currentUser);
            bool isBoss = currentAdmin?.Role == "Boss";
            int? myStoreId = currentAdmin?.StoreId;

            var query = _context.TinNhans.AsNoTracking().AsQueryable();

            if (isBoss)
            {
                // Boss thấy tất cả
                query = query.Where(m => m.SenderName == currentUser || m.ReceiverName == currentUser || m.ReceiverName == "Admin" || m.SenderName == "Admin");

                // Lấy cả tin nhắn nội bộ giữa các Admin để Boss giám sát (nếu cần)
                var allAdmins = _context.TaiKhoans.Where(t => t.Role == "Admin").Select(t => t.Username).ToList();
                query = query.Where(m =>
                    allAdmins.Contains(m.SenderName) || allAdmins.Contains(m.ReceiverName) ||
                    m.SenderName == currentUser || m.ReceiverName == currentUser || m.ReceiverName == "Admin"
                );

                if (filterStoreId.HasValue && filterStoreId.Value > 0)
                {
                    query = query.Where(m => m.StoreId == filterStoreId.Value);
                }
            }
            else
            {
                // Admin thấy tin của mình hoặc tin hệ thống đúng Store
                query = query.Where(m =>
                    m.SenderName == currentUser ||
                    m.ReceiverName == currentUser ||
                    ((m.ReceiverName == "Admin" || m.SenderName == "Admin") && (m.StoreId == myStoreId || m.StoreId == null))
                );
            }

            var allMessages = query.OrderByDescending(m => m.Timestamp).ToList();

            // Lọc danh sách người đã chat
            var contactNames = new HashSet<string>();
            foreach (var m in allMessages)
            {
                string other = (m.SenderName == currentUser || m.SenderName == "Admin") ? m.ReceiverName : m.SenderName;
                if (other != "Admin" && other != currentUser) contactNames.Add(other);
            }

            // --- [LOGIC GHIM DANH BẠ NỘI BỘ] ---

            // 1. Nếu là BOSS: Luôn thấy tất cả Admin (để ghim lên đầu)
            if (isBoss && (!filterStoreId.HasValue || filterStoreId == 0))
            {
                var adminUsers = _context.TaiKhoans.Where(u => u.Role == "Admin").Select(u => u.Username).ToList();
                foreach (var admin in adminUsers) contactNames.Add(admin);
            }

            // 2. [MỚI] Nếu là ADMIN:
            if (!isBoss)
            {
                // - Luôn thấy Boss
                var bossUsers = _context.TaiKhoans.Where(u => u.Role == "Boss").Select(u => u.Username).ToList();
                foreach (var boss in bossUsers) contactNames.Add(boss);

                // - Luôn thấy các Admin khác (Đồng nghiệp)
                var otherAdmins = _context.TaiKhoans
                    .Where(u => u.Role == "Admin" && u.Username != currentUser)
                    .Select(u => u.Username)
                    .ToList();
                foreach (var admin in otherAdmins) contactNames.Add(admin);
            }

            var contactList = new List<object>();

            foreach (var otherUser in contactNames)
            {
                var lastMsg = allMessages.FirstOrDefault(m => m.SenderName == otherUser || m.ReceiverName == otherUser);
                int unread = allMessages.Count(m => m.SenderName == otherUser && !m.IsRead && (m.ReceiverName == currentUser || m.ReceiverName == "Admin"));

                var acc = _context.TaiKhoans.AsNoTracking().FirstOrDefault(a => a.Username == otherUser);
                string displayName = acc?.FullName ?? otherUser;
                string role = acc?.Role ?? "User";

                // --- [LOGIC SẮP XẾP PRIORITY] ---
                int priority = 99;

                if (isBoss)
                {
                    // Boss view: Admin (1) -> Khách (99)
                    if (role == "Admin") priority = 1;
                    else priority = 99;
                }
                else
                {
                    // Admin view: Boss (1) -> Admin khác (2) -> Khách (99)
                    if (role == "Boss") priority = 1;
                    else if (role == "Admin") priority = 2;
                    else priority = 99;
                }

                contactList.Add(new
                {
                    username = otherUser,
                    fullName = displayName,
                    lastMessage = lastMsg?.Content ?? "Chưa có tin nhắn",
                    time = lastMsg?.Timestamp.ToString("HH:mm dd/MM") ?? "",
                    unread = unread,
                    priority = priority,
                    role = role
                });
            }

            // Sắp xếp: Priority nhỏ lên đầu -> Sau đó đến tin mới nhất
            return Json(contactList.OrderBy(x => ((dynamic)x).priority).ThenByDescending(x => ((dynamic)x).time));
        }

        // --- 2. API GET HISTORY ---
        [HttpGet]
        public IActionResult GetHistory(string otherUser, int? storeId)
        {
            var currentUser = User.Identity.Name;
            if (!string.IsNullOrEmpty(otherUser)) otherUser = System.Net.WebUtility.UrlDecode(otherUser);

            var currentAdmin = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == currentUser);
            bool isBoss = currentAdmin?.Role == "Boss";
            bool isAdmin = currentAdmin?.Role == "Admin";

            var query = _context.TinNhans.AsQueryable();

            if (isBoss)
            {
                // Boss thấy TẤT CẢ tin liên quan đến 'otherUser'
                query = query.Where(m => m.SenderName == otherUser || m.ReceiverName == otherUser);
            }
            else if (isAdmin)
            {
                int? myStoreId = currentAdmin?.StoreId;

                query = query.Where(m =>
                    // Chat riêng (Admin <-> Boss/Admin/Khách)
                    (m.SenderName == currentUser && m.ReceiverName == otherUser) ||
                    (m.SenderName == otherUser && m.ReceiverName == currentUser) ||

                    // Chat hệ thống (Admin <-> Khách) - Chỉ lấy đúng Store hoặc tin cũ
                    ((m.SenderName == "Admin" || m.ReceiverName == "Admin") &&
                     (m.SenderName == otherUser || m.ReceiverName == otherUser) &&
                     (m.StoreId == myStoreId || m.StoreId == null))
                );
            }
            else
            {
                // Khách hàng
                query = query.Where(m => m.SenderName == currentUser || m.ReceiverName == currentUser);
                if (storeId.HasValue && storeId.Value > 0)
                {
                    query = query.Where(m => m.StoreId == storeId.Value);
                }
            }

            if (isBoss || isAdmin)
            {
                var unread = query.Where(m => !m.IsRead && m.SenderName == otherUser).ToList();
                if (unread.Any()) { foreach (var m in unread) m.IsRead = true; _context.SaveChanges(); }
            }

            return Json(query.OrderBy(m => m.Timestamp).Select(m => new { sender = m.SenderName, content = m.Content, time = m.Timestamp.ToString("HH:mm dd/MM") }).ToList());
        }
    }
}