using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace banthietbidientu.Services
{
    public class MemberTier
    {
        public string Name { get; set; } = "Thành Viên";
        public string Color { get; set; } = "secondary";
        public string Icon { get; set; } = "bi-person";
        public string CssClass { get; set; } = "tier-member";
        public decimal TotalSpent { get; set; } = 0;
        public int DiscountPercent { get; set; } = 0;
    }

    public class MemberService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public MemberService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public MemberTier GetUserTier(string username)
        {
            if (string.IsNullOrEmpty(username)) return new MemberTier();

            string cacheKey = $"UserTier_{username}";

            if (!_cache.TryGetValue(cacheKey, out MemberTier cachedTier))
            {
                cachedTier = CalculateTierFromDb(username);

                // Lưu cache trong 5 phút để giảm tải DB
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, cachedTier, cacheEntryOptions);
            }

            return cachedTier;
        }

        private MemberTier CalculateTierFromDb(string username)
        {
            var tier = new MemberTier();
            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == username);
            if (user == null) return tier;

            // 1. CẤP LÃNH ĐẠO (Dựa vào Role) - ƯU TIÊN KIỂM TRA TRƯỚC
            if (user.Role == "Boss")
            {
                return new MemberTier
                {
                    Name = "👑 BOSS (CHỦ TỊCH)",
                    Color = "danger", // Màu đỏ quyền lực
                    Icon = "bi-shield-lock-fill", // Icon khiên bảo vệ
                    CssClass = "tier-boss",
                    TotalSpent = 9999999999, // Số tiền tượng trưng cực lớn
                    DiscountPercent = 50 // Giảm 50%
                };
            }

            if (user.Role == "Admin")
            {
                return new MemberTier
                {
                    Name = "🛡️ ADMIN (QUẢN LÝ)",
                    Color = "primary", // Màu xanh dương đậm
                    Icon = "bi-person-badge-fill", // Icon thẻ nhân viên
                    CssClass = "tier-admin",
                    TotalSpent = 5000000000,
                    DiscountPercent = 25 // Giảm 25%
                };
            }

            // 2. CẤP KHÁCH HÀNG (Nếu không phải Boss/Admin thì mới tính chi tiêu)
            // Chỉ tính tổng tiền các đơn hàng ĐÃ HOÀN THÀNH (TrangThai == 3)
            var totalSpent = _context.DonHangs
                 .Where(d => d.TaiKhoanId == user.Id && d.TrangThai == 3)
                 .Sum(d => d.TongTien);

            tier.TotalSpent = totalSpent;

            if (totalSpent >= 100_000_000) // KIM CƯƠNG
            {
                tier.Name = "💎 KIM CƯƠNG";
                tier.Color = "dark"; // Màu đen sang trọng
                tier.Icon = "bi-gem";
                tier.CssClass = "tier-diamond";
                tier.DiscountPercent = 10;
            }
            else if (totalSpent >= 50_000_000) // BẠCH KIM
            {
                tier.Name = "🏆 BẠCH KIM";
                tier.Color = "info"; // Màu xanh ngọc
                tier.Icon = "bi-trophy-fill";
                tier.CssClass = "tier-platinum";
                tier.DiscountPercent = 5;
            }
            else if (totalSpent >= 20_000_000) // VÀNG
            {
                tier.Name = "🥇 VÀNG";
                tier.Color = "warning"; // Màu vàng
                tier.Icon = "bi-award-fill";
                tier.CssClass = "tier-gold";
                tier.DiscountPercent = 0;
            }
            else if (totalSpent >= 10_000_000) // BẠC
            {
                tier.Name = "🥈 BẠC";
                tier.Color = "secondary"; // Màu xám
                tier.Icon = "bi-medal-fill";
                tier.CssClass = "tier-silver";
                tier.DiscountPercent = 0;
            }
            else // Thành viên thường
            {
                tier.Name = "THÀNH VIÊN";
                tier.Color = "light text-dark border"; // Màu sáng có viền
                tier.Icon = "bi-person-circle";
                tier.CssClass = "tier-member";
                tier.DiscountPercent = 0;
            }

            return tier;
        }

        // --- HÀM MỚI: XÓA CACHE CỦA USER ---
        // Được gọi từ AdminController khi cập nhật trạng thái đơn hàng thành công/hủy
        public void ClearUserCache(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                string cacheKey = $"UserTier_{username}";
                _cache.Remove(cacheKey); // Xóa ngay lập tức bộ nhớ đệm của user này
            }
        }
    }
}