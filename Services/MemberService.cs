using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Models;

namespace banthietbidientu.Services
{
    // Class chứa thông tin hiển thị hạng
    public class MemberTier
    {
        public string Name { get; set; } = "Thành Viên"; // Tên hạng
        public string Color { get; set; } = "secondary"; // Màu Bootstrap (hoặc mã Hex)
        public string Icon { get; set; } = "bi-person";  // Icon Bootstrap
        public string CssClass { get; set; } = "tier-member"; // Class CSS riêng
        public decimal TotalSpent { get; set; } = 0; // Tổng tiền đã chi
    }

    public class MemberService
    {
        private readonly ApplicationDbContext _context;

        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public MemberTier GetUserTier(string username)
        {
            var tier = new MemberTier();

            if (string.IsNullOrEmpty(username)) return tier;

            // 1. Lấy User ID từ Username
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == username);
            if (user == null) return tier;

            // --- [LOGIC MỚI] ƯU TIÊN ADMIN ---
            // Nếu là Admin -> Trả về hạng ĐẶC BIỆT ngay lập tức (Class tier-admin)
            if (user.Role == "Admin")
            {
                return new MemberTier
                {
                    Name = "👑 TỐI THƯỢNG",
                    Color = "danger",
                    Icon = "bi-shield-lock-fill",
                    CssClass = "tier-admin",
                    TotalSpent = 9999999999
                };
            }
            // --------------------------------

            // 2. Tính tổng tiền đã mua (Chỉ tính đơn hàng Hoàn thành)
            var totalSpent = _context.DonHangs
                 .Where(d => d.TaiKhoanId == user.Id && d.TrangThai == 3)
                 .Sum(d => d.TongTien);

            tier.TotalSpent = totalSpent;

            // 3. Phân hạng theo quy ước
            if (totalSpent >= 100_000_000) // 100 Triệu - KIM CƯƠNG
            {
                tier.Name = "💎 KIM CƯƠNG";
                tier.Color = "dark";
                tier.Icon = "bi-gem";
                tier.CssClass = "tier-diamond";
            }
            else if (totalSpent >= 50_000_000) // 50 Triệu - BẠCH KIM
            {
                tier.Name = "🏆 BẠCH KIM";
                tier.Color = "info";
                tier.Icon = "bi-trophy-fill";
                tier.CssClass = "tier-platinum";
            }
            else if (totalSpent >= 20_000_000) // 20 Triệu - VÀNG
            {
                tier.Name = "🥇 VÀNG";
                tier.Color = "warning";
                tier.Icon = "bi-award-fill";
                tier.CssClass = "tier-gold";
            }
            else if (totalSpent >= 10_000_000) // 10 Triệu - BẠC
            {
                tier.Name = "🥈 BẠC";
                tier.Color = "secondary";
                tier.Icon = "bi-medal-fill";
                tier.CssClass = "tier-silver";
            }
            else // Thành viên thường
            {
                tier.Name = "THÀNH VIÊN";
                tier.Color = "light";
                tier.Icon = "bi-person-circle";
                tier.CssClass = "tier-member";
            }

            return tier;
        }
    }
}