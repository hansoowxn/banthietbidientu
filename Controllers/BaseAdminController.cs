using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services;
using System;
using System.Linq;

namespace banthietbidientu.Controllers
{
    [Authorize(Roles = "Admin,Boss")]
    public class BaseAdminController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IEmailSender _emailSender;

        public BaseAdminController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // Helper: Ghi nhật ký hoạt động
        protected void GhiNhatKy(string hanhDong, string noiDung)
        {
            try
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    var log = new LichSuHoatDong
                    {
                        TaiKhoanId = user.Id,
                        TenNguoiDung = user.FullName ?? user.Username,
                        StoreId = user.StoreId,
                        HanhDong = hanhDong,
                        NoiDung = noiDung,
                        ThoiGian = DateTime.Now
                    };
                    _context.LichSuHoatDongs.Add(log);
                    _context.SaveChanges();
                }
            }
            catch { /* Ignore log error */ }
        }

        // Helper: Tạo thông báo hệ thống
        protected void TaoThongBaoHeThong(string tieuDe, string noiDung, string redirectId, string action, int? storeId)
        {
            var tb = new ThongBao
            {
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 4, // 4: Thông báo hệ thống/Kho
                RedirectId = redirectId,
                RedirectAction = action,
                StoreId = storeId
            };
            _context.ThongBaos.Add(tb);
            _context.SaveChanges();
        }

        // Helper: Lấy tên kho hiển thị
        protected string GetStoreName(int storeId)
        {
            return storeId switch { 1 => "Hà Nội", 2 => "Đà Nẵng", 3 => "TP.HCM", _ => "Kho ?" };
        }
    }
}