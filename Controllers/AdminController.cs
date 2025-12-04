using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading.Tasks;

namespace banthietbidientu.Controllers
{
    [Authorize(Roles = "Admin,Boss")]
    public class AdminController : BaseAdminController
    {
        // Constructor kế thừa từ BaseAdminController
        public AdminController(ApplicationDbContext context, IEmailSender emailSender) : base(context, emailSender)
        {
        }

        // --- 1. DASHBOARD (THỐNG KÊ) ---
        public IActionResult Index()
        {
            var user = _context.TaiKhoans.AsNoTracking()
                .FirstOrDefault(u => u.Username == User.Identity.Name);

            ViewBag.CurrentStoreId = user?.StoreId ?? 0;
            int? storeId = user?.StoreId;
            var queryDonHang = _context.DonHangs.AsQueryable();

            if (storeId.HasValue)
            {
                queryDonHang = queryDonHang.Where(d => d.StoreId == storeId);
            }

            var today = DateTime.Today;

            ViewBag.DoanhThuHomNay = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Date == today)
                .Sum(x => x.TongTien);

            ViewBag.DonHangHomNay = queryDonHang
                .Count(d => d.NgayDat.Value.Date == today);

            ViewBag.DonChoXuLy = queryDonHang
                .Count(d => d.TrangThai == 0 || d.TrangThai == 1);

            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            decimal revThisMonth = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat >= startOfMonth)
                .Sum(x => x.TongTien);

            decimal revLastMonth = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat >= startOfLastMonth && d.NgayDat <= endOfLastMonth)
                .Sum(x => x.TongTien);

            double growth = 0;
            if (revLastMonth > 0)
            {
                growth = (double)((revThisMonth - revLastMonth) / revLastMonth) * 100;
            }
            else if (revThisMonth > 0)
            {
                growth = 100;
            }

            ViewBag.RevenueThisMonth = revThisMonth;
            ViewBag.RevenueGrowth = growth;
            ViewBag.TongDoanhThu = queryDonHang.Where(d => d.TrangThai == 3).Sum(x => x.TongTien);
            ViewBag.TongDonHang = queryDonHang.Count();
            ViewBag.TongKhachHang = _context.TaiKhoans.Count(x => x.Role == "User");

            var chartLabels = new List<string>();
            var chartRevenue = new List<decimal>();
            var chartOrders = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                chartLabels.Add(day.ToString("dd/MM"));
                var dailyData = queryDonHang.Where(x => x.NgayDat.Value.Date == day);

                chartRevenue.Add(dailyData.Where(x => x.TrangThai == 3).Sum(x => (decimal?)x.TongTien) ?? 0);
                chartOrders.Add(dailyData.Count());
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartRevenue = chartRevenue;
            ViewBag.ChartOrders = chartOrders;

            ViewBag.PieCompleted = queryDonHang.Count(x => x.TrangThai == 3);
            ViewBag.PieShipping = queryDonHang.Count(x => x.TrangThai == 2);
            ViewBag.PiePending = queryDonHang.Count(x => x.TrangThai == 0 || x.TrangThai == 1);
            ViewBag.PieCancelled = queryDonHang.Count(x => x.TrangThai == -1);

            var recentOrders = queryDonHang
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .Select(d => new DonHangViewModel
                {
                    MaDon = d.MaDon,
                    TenKhachHang = d.NguoiNhan,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai == 1 ? "Đã xác nhận" :
                                d.TrangThai == 2 ? "Đang giao" :
                                d.TrangThai == 3 ? "Hoàn thành" :
                                d.TrangThai == -1 ? "Đã hủy" : "Chờ xử lý"
                })
                .ToList();

            return View(recentOrders);
        }

        // --- 2. QUẢN LÝ ĐƠN HÀNG ---
        public IActionResult QuanLyDonHang(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var user = _context.TaiKhoans.AsNoTracking()
                .FirstOrDefault(u => u.Username == User.Identity.Name);

            int? storeId = user?.StoreId;

            var query = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(d => d.StoreId == storeId);
            }

            query = query.OrderByDescending(d => d.NgayDat);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var listDonHang = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DonHangViewModel
                {
                    MaDon = d.MaDon,
                    TenKhachHang = d.NguoiNhan,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai == 1 ? "Đã xác nhận" :
                                d.TrangThai == 2 ? "Đang giao" :
                                d.TrangThai == 3 ? "Hoàn thành" :
                                d.TrangThai == -1 ? "Đã hủy" : "Chờ xử lý",
                    StoreId = d.StoreId,
                    SanPhams = d.ChiTietDonHangs.Select(ct => new DonHangViewModel
                    {
                        TenSanPham = ct.SanPham.Name,
                        HinhAnh = ct.SanPham.ImageUrl,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia
                    }).ToList()
                })
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(listDonHang);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(string orderId, string status)
        {
            try
            {
                var donHang = await _context.DonHangs
                                      .Include(d => d.TaiKhoan)
                                      .FirstOrDefaultAsync(x => x.MaDon == orderId);

                if (donHang == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy đơn {orderId}" });
                }

                string trangThaiCu = donHang.TrangThai == 1 ? "Đã xác nhận" :
                                     donHang.TrangThai == 2 ? "Đang giao" :
                                     donHang.TrangThai == 3 ? "Hoàn thành" :
                                     donHang.TrangThai == -1 ? "Đã hủy" : "Chờ xử lý";

                int statusInt = 0;
                if (status == "Đã xác nhận") statusInt = 1;
                else if (status == "Đang giao") statusInt = 2;
                else if (status == "Hoàn thành") statusInt = 3;
                else if (status == "Đã hủy") statusInt = -1;

                donHang.TrangThai = statusInt;
                await _context.SaveChangesAsync();

                GhiNhatKy("Cập nhật đơn hàng", $"Đổi trạng thái đơn {orderId} từ '{trangThaiCu}' sang '{status}'");

                if (statusInt == 3)
                {
                    string userEmail = donHang.TaiKhoan?.Email;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        string subject = $"[SmartTech] Đơn hàng #{donHang.MaDon} đã giao thành công!";
                        string body = $@"
                            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                                <h3 style='color: #28a745;'>Cảm ơn bạn đã mua sắm tại SmartTech!</h3>
                                <p>Xin chào <strong>{donHang.NguoiNhan}</strong>,</p>
                                <p>Đơn hàng <strong>#{donHang.MaDon}</strong> của bạn đã được giao thành công.</p>
                                <p>Chúng tôi hy vọng bạn hài lòng với sản phẩm.</p>
                            </div>";

                        try { await _emailSender.SendEmailAsync(userEmail, subject, body); } catch { }
                    }
                }

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // --- 4. API THÔNG BÁO ---
        [HttpGet]
        public IActionResult GetThongBao()
        {
            try
            {
                var user = _context.TaiKhoans.AsNoTracking()
                    .FirstOrDefault(u => u.Username == User.Identity.Name);

                int? storeId = user?.StoreId;
                bool isBoss = user?.Role == "Boss";

                var query = _context.ThongBaos.AsQueryable();

                if (!isBoss && storeId.HasValue)
                {
                    query = query.Where(t => t.StoreId == storeId);
                }

                var thongBaos = query
                    .OrderByDescending(x => x.NgayTao)
                    .Take(5)
                    .Select(x => new
                    {
                        id = x.Id,
                        tieuDe = x.TieuDe,
                        noiDung = x.NoiDung,
                        ngayTao = x.NgayTao,
                        daDoc = x.DaDoc,
                        loaiThongBao = x.LoaiThongBao,
                        redirectId = x.RedirectId,
                        redirectAction = x.RedirectAction,
                        controller = (x.RedirectAction == "QuanLyChuyenKho" || x.RedirectAction == "QuanLyNhapHang") ? "Warehouse" :
                     (x.RedirectAction == "QuanLySanPham" || x.RedirectAction == "Index" && x.TieuDe.Contains("Sản phẩm")) ? "ProductManager" :
                     "Admin"
                    })
                    .ToList();

                var soChuaDoc = query.Count(x => !x.DaDoc);

                return Json(new { success = true, data = thongBaos, unread = soChuaDoc });
            }
            catch
            {
                return Json(new { success = true, data = new List<object>(), unread = 0 });
            }
        }

        [HttpPost]
        public IActionResult MarkAsRead()
        {
            var list = _context.ThongBaos.Where(x => !x.DaDoc).ToList();
            foreach (var item in list) item.DaDoc = true;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult MarkOneRead(int id)
        {
            var tb = _context.ThongBaos.Find(id);
            if (tb != null)
            {
                tb.DaDoc = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public IActionResult QuanLyThongBao()
        {
            var list = _context.ThongBaos.OrderByDescending(x => x.NgayTao).ToList();
            return View(list);
        }

        // --- 6. QUẢN LÝ TÀI KHOẢN ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyTaiKhoan()
        {
            return View(_context.TaiKhoans.AsNoTracking().ToList());
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult ThemTaiKhoan()
        {
            return View();
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult ThemTaiKhoan(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                _context.TaiKhoans.Add(model);
                _context.SaveChanges();
                GhiNhatKy("Thêm tài khoản", $"Tạo mới tài khoản: {model.Username} ({model.Role})");
                return RedirectToAction("QuanLyTaiKhoan");
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaTaiKhoan(int id)
        {
            var tk = _context.TaiKhoans.Find(id);
            return tk == null ? NotFound() : View(tk);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult SuaTaiKhoan(TaiKhoan model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("DonHangs");
            if (ModelState.IsValid)
            {
                var tk = _context.TaiKhoans.Find(model.Id);
                if (tk != null)
                {
                    tk.FullName = model.FullName;
                    tk.Email = model.Email;
                    tk.Role = model.Role;
                    tk.Address = model.Address;
                    tk.PhoneNumber = model.PhoneNumber;
                    _context.SaveChanges();
                    GhiNhatKy("Sửa tài khoản", $"Cập nhật tài khoản: {tk.Username}");
                }
                return RedirectToAction("QuanLyTaiKhoan");
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaTaiKhoan(int id)
        {
            var tk = _context.TaiKhoans.Find(id);
            if (tk != null)
            {
                string tenUser = tk.Username;
                _context.TaiKhoans.Remove(tk);
                _context.SaveChanges();
                GhiNhatKy("Xóa tài khoản", $"Đã xóa tài khoản: {tenUser}");
            }
            return RedirectToAction("QuanLyTaiKhoan");
        }

        // --- 7. BÁO CÁO ---
        [Authorize(Roles = "Boss")]
        public IActionResult BaoCao(int? storeId, int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;
            int prevMonth = selectedMonth == 1 ? 12 : selectedMonth - 1;
            int prevYearOfMonth = selectedMonth == 1 ? selectedYear - 1 : selectedYear;

            string storeName = "Toàn hệ thống";
            if (storeId == 1) storeName = "Chi nhánh Hà Nội";
            else if (storeId == 2) storeName = "Chi nhánh Đà Nẵng";
            else if (storeId == 3) storeName = "Chi nhánh TP.HCM";

            List<decimal> GetMonthlyData(int y)
            {
                var data = new List<decimal>();
                for (int m = 1; m <= 12; m++)
                {
                    var monthData = _context.DonHangs
                        .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == y && d.NgayDat.Value.Month == m && (!storeId.HasValue || d.StoreId == storeId))
                        .Select(d => new { d.TongTien, d.TienThue }).ToList();
                    decimal netRevenue = monthData.Sum(x => x.TongTien - x.TienThue);
                    data.Add(netRevenue);
                }
                return data;
            }

            double CalcGrowth(decimal cur, decimal prev) => prev > 0 ? (double)((cur - prev) / prev) * 100 : 100;

            (decimal DoanhThu, int DonHang, int SanPham, decimal LoiNhuan) CalculateKpi(int m, int y)
            {
                var queryDH = _context.DonHangs.Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == y && d.NgayDat.Value.Month == m && (!storeId.HasValue || d.StoreId == storeId));
                var queryCT = _context.ChiTietDonHangs.Where(ct => ct.DonHang.TrangThai == 3 && ct.DonHang.NgayDat.Value.Year == y && ct.DonHang.NgayDat.Value.Month == m && (!storeId.HasValue || ct.DonHang.StoreId == storeId));

                decimal tongThu = queryDH.Sum(d => d.TongTien);
                decimal tongThue = queryDH.Sum(d => d.TienThue);
                decimal thucThu = tongThu - tongThue;
                decimal giaVon = queryCT.Sum(ct => ct.GiaGoc * ct.SoLuong);
                return (thucThu, queryDH.Count(), queryCT.Sum(ct => ct.SoLuong), thucThu - giaVon);
            }

            var current = CalculateKpi(selectedMonth, selectedYear);
            var previous = CalculateKpi(prevMonth, prevYearOfMonth);

            var categoryData = _context.ChiTietDonHangs.Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang.TrangThai == 3 && ct.DonHang.NgayDat.Value.Year == selectedYear && ct.DonHang.NgayDat.Value.Month == selectedMonth && (!storeId.HasValue || ct.DonHang.StoreId == storeId))
                .GroupBy(ct => ct.SanPham.Category)
                .Select(g => new { Name = g.Key, Value = g.Sum(x => (decimal?)x.Gia * x.SoLuong) ?? 0 })
                .OrderByDescending(x => x.Value).ToList();

            var storeComparison = new List<decimal>();
            if (!storeId.HasValue)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var storeData = _context.DonHangs.Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == selectedYear && d.NgayDat.Value.Month == selectedMonth && d.StoreId == i).Select(d => new { d.TongTien, d.TienThue }).ToList();
                    storeComparison.Add(storeData.Sum(x => x.TongTien - x.TienThue));
                }
            }

            var queryChiTietFull = _context.ChiTietDonHangs.Include(ct => ct.SanPham).Where(ct => ct.DonHang.TrangThai == 3 && ct.DonHang.NgayDat.Value.Year == selectedYear && ct.DonHang.NgayDat.Value.Month == selectedMonth && (!storeId.HasValue || ct.DonHang.StoreId == storeId));
            decimal tongDoanhThuNiemYet = queryChiTietFull.Sum(ct => (ct.Gia ?? 0) * ct.SoLuong);
            decimal tyLeThucThu = tongDoanhThuNiemYet > 0 ? (current.DoanhThu / tongDoanhThuNiemYet) : 1;

            var baoCaoLoiNhuan = queryChiTietFull.GroupBy(ct => new { ct.SanPhamId, ct.SanPham.Name, ct.SanPham.ImageUrl })
                .Select(g => new { Ten = g.Key.Name, Hinh = g.Key.ImageUrl, Sl = g.Sum(x => x.SoLuong), DtNiemYet = g.Sum(x => (x.Gia ?? 0) * x.SoLuong), Gv = g.Sum(x => x.GiaGoc * x.SoLuong) }).ToList()
                .Select(x => new LoiNhuanSanPham { TenSanPham = x.Ten, HinhAnh = x.Hinh, SoLuongBan = x.Sl, DoanhThuNiemYet = x.DtNiemYet, GiaVon = x.Gv, DoanhThuThuc = x.DtNiemYet * tyLeThucThu, LoiNhuan = (x.DtNiemYet * tyLeThucThu) - x.Gv })
                .OrderByDescending(x => x.DoanhThuThuc).ToList();

            var vipUsers = _context.TaiKhoans.Where(u => u.Role == "User").Select(u => new { User = u, TotalSpent = u.DonHangs.Where(d => d.TrangThai == 3).Sum(d => (decimal?)d.TongTien - d.TienThue) ?? 0, OrderCount = u.DonHangs.Count(d => d.TrangThai == 3) }).Where(x => x.TotalSpent >= 100000000).OrderByDescending(x => x.TotalSpent).Take(10).Select(x => new KhachHangTiemNang { Id = x.User.Id, HoTen = x.User.FullName, Username = x.User.Username, SoDienThoai = x.User.PhoneNumber, TongChiTieu = x.TotalSpent, SoDonHang = x.OrderCount }).ToList();

            var model = new BaoCaoViewModel { TongDoanhThu = current.DoanhThu, TongDoanhThuGoc = tongDoanhThuNiemYet, TongDonHang = current.DonHang, TongSanPhamDaBan = current.SanPham, LoiNhuanUocTinh = current.LoiNhuan, SanPhamSapHet = _context.SanPhams.Count(s => s.SoLuong < 5), GrowthDoanhThu = CalcGrowth(current.DoanhThu, previous.DoanhThu), GrowthLoiNhuan = CalcGrowth(current.LoiNhuan, previous.LoiNhuan), GrowthDonHang = CalcGrowth(current.DonHang, previous.DonHang), GrowthSanPham = CalcGrowth(current.SanPham, previous.SanPham), DataNamHienTai = GetMonthlyData(selectedYear), DataNamTruoc = GetMonthlyData(selectedYear - 1), DataNamKia = GetMonthlyData(selectedYear - 2), CurrentYear = selectedYear, CategoryLabels = categoryData.Select(x => x.Name).ToList(), CategoryValues = categoryData.Select(x => x.Value).ToList(), StoreRevenueComparison = storeComparison, SelectedStoreId = storeId, StoreName = storeName, BaoCaoLoiNhuan = baoCaoLoiNhuan, KhachHangTiemNangs = vipUsers };
            ViewBag.SelectedMonth = selectedMonth; ViewBag.SelectedYear = selectedYear; ViewBag.IsBoss = true;
            return View(model);
        }

        // --- 8. XUẤT EXCEL & IN HÓA ĐƠN ---
        [HttpGet]
        public IActionResult XuatExcelDonHang()
        {
            var orders = _context.DonHangs.Include(d => d.TaiKhoan).OrderByDescending(d => d.NgayDat).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Mã đơn hàng,Ngày đặt,Khách hàng,Số điện thoại,Địa chỉ giao hàng,Tổng tiền,Trạng thái");
            foreach (var item in orders)
            {
                string trangThaiText = item.TrangThai switch { 0 => "Chờ xử lý", 1 => "Đã xác nhận", 2 => "Đang giao", 3 => "Hoàn thành", -1 => "Đã hủy", _ => "Khác" };
                string diaChiSafe = item.DiaChi?.Replace("\"", "\"\"") ?? "";
                string ngayDatStr = item.NgayDat.HasValue ? item.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "";
                sb.AppendLine($"{item.MaDon},{ngayDatStr},{item.NguoiNhan},{item.SDT},\"{diaChiSafe}\",{item.TongTien},{trangThaiText}");
            }
            var fileContent = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(fileContent, "text/csv", $"BaoCaoDonHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        [HttpGet]
        public IActionResult InHoaDon(string orderId)
        {
            var order = _context.DonHangs.Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham).Include(d => d.TaiKhoan).FirstOrDefault(d => d.MaDon == orderId);
            if (order == null) return NotFound();
            string shopAddress = "120 Xuân Thủy, Cầu Giấy, Hà Nội";
            string zoneCode = "HN-CG-01";
            string addr = (order.DiaChi ?? "").ToLower();
            string[] mienTrung = { "đà nẵng", "huế", "quảng nam", "quảng ngãi", "bình định", "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa" };
            string[] mienNam = { "hồ chí minh", "tp.hcm", "hcm", "sài gòn", "bình dương", "đồng nai", "bà rịa", "vũng tàu", "long an", "tiền giang", "cần thơ" };
            if (mienNam.Any(k => addr.Contains(k))) { shopAddress = "55 Nguyễn Huệ, Quận 1, TP.HCM"; zoneCode = "SG-Q1-03"; }
            else if (mienTrung.Any(k => addr.Contains(k))) { shopAddress = "78 Bạch Đằng, Hải Châu, Đà Nẵng"; zoneCode = "DN-HC-02"; }
            ViewBag.ShopAddress = shopAddress; ViewBag.ZoneCode = zoneCode;
            var model = new DonHangViewModel { MaDon = order.MaDon, NgayDat = order.NgayDat, TenKhachHang = !string.IsNullOrEmpty(order.NguoiNhan) ? order.NguoiNhan : (order.TaiKhoan?.FullName ?? "Khách lẻ"), SoDienThoai = !string.IsNullOrEmpty(order.SDT) ? order.SDT : (order.TaiKhoan?.PhoneNumber ?? ""), DiaChiGiaoHang = order.DiaChi ?? "", TongTien = order.TongTien, PhuongThucThanhToan = "Thanh toán khi nhận hàng (COD)", SanPhams = order.ChiTietDonHangs.Select(ct => new DonHangViewModel { TenSanPham = ct.SanPham.Name, SoLuong = ct.SoLuong, Gia = ct.Gia }).ToList() };
            return View(model);
        }

        // --- CÁC HÀM KHÁC ---

        public IActionResult QuanLyDanhGia()
        {
            var listDanhGia = _context.DanhGias.Include(d => d.SanPham).Include(d => d.TaiKhoan).OrderByDescending(d => d.NgayTao).ToList();
            return View(listDanhGia);
        }

        [HttpPost]
        public IActionResult DuyetDanhGia(int id)
        {
            var d = _context.DanhGias.Find(id);
            if (d != null)
            {
                d.DaDuyet = !d.DaDuyet;
                _context.SaveChanges();
                GhiNhatKy("Duyệt đánh giá", $"Thay đổi trạng thái duyệt cho đánh giá ID {id}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult TraLoiDanhGia(int id, string noiDungTraLoi)
        {
            var d = _context.DanhGias.Find(id);
            if (d != null)
            {
                d.TraLoi = noiDungTraLoi;
                d.NgayTraLoi = DateTime.Now;
                _context.SaveChanges();
                GhiNhatKy("Trả lời đánh giá", $"Đã trả lời đánh giá ID {id} của khách hàng");
                return RedirectToAction("QuanLyDanhGia");
            }
            return RedirectToAction("QuanLyDanhGia");
        }

        [HttpPost]
        public IActionResult XoaDanhGia(int id)
        {
            var d = _context.DanhGias.Find(id);
            if (d != null)
            {
                _context.DanhGias.Remove(d);
                _context.SaveChanges();
                GhiNhatKy("Xóa đánh giá", $"Đã xóa đánh giá ID {id}");
            }
            return RedirectToAction("QuanLyDanhGia");
        }

        public IActionResult QuanLyThuMua()
        {
            var listYeuCau = _context.YeuCauThuMuas.Include(y => y.TaiKhoan).OrderByDescending(y => y.NgayTao).ToList();
            return View(listYeuCau);
        }

        [HttpPost]
        public IActionResult CapNhatThuMua(int id, int trangThai, string ghiChuAdmin)
        {
            var y = _context.YeuCauThuMuas.Find(id);
            if (y != null)
            {
                y.TrangThai = trangThai;
                y.GhiChu = ghiChuAdmin;
                _context.SaveChanges();
                GhiNhatKy("Cập nhật thu mua", $"Yêu cầu {y.TenMay} (ID: {id}) -> Trạng thái {trangThai}");
            }
            return RedirectToAction("QuanLyThuMua");
        }

        // --- 11. QUẢN LÝ BANNER ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyBanner()
        {
            var banners = _context.Banners.OrderBy(b => b.DisplayOrder).ToList();
            return View(banners);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult ThemBanner()
        {
            return View();
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemBanner(Banner model)
        {
            var maxOrder = _context.Banners.Any() ? _context.Banners.Max(b => b.DisplayOrder) : 0;
            model.DisplayOrder = maxOrder + 1;
            ModelState.Remove("DisplayOrder");
            if (ModelState.IsValid)
            {
                _context.Banners.Add(model);
                _context.SaveChanges();
                GhiNhatKy("Thêm Banner", $"Thêm banner mới: {model.Title}");
                TempData["Success"] = "Thêm banner mới thành công!";
                return RedirectToAction("QuanLyBanner");
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner == null) return NotFound();
            return View("SuaBanner", banner);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaBanner(Banner model)
        {
            ModelState.Remove("DisplayOrder");
            if (ModelState.IsValid)
            {
                var existingBanner = _context.Banners.Find(model.Id);
                if (existingBanner != null)
                {
                    existingBanner.ImageUrl = model.ImageUrl;
                    existingBanner.Title = model.Title;
                    existingBanner.Description = model.Description;
                    existingBanner.ButtonText = model.ButtonText;
                    existingBanner.LinkUrl = model.LinkUrl;
                    existingBanner.IsActive = model.IsActive;
                    _context.SaveChanges();
                    GhiNhatKy("Sửa Banner", $"Cập nhật banner ID: {model.Id}");
                    TempData["Success"] = "Cập nhật banner thành công!";
                    return RedirectToAction("QuanLyBanner");
                }
                else return NotFound();
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner != null)
            {
                string title = banner.Title;
                _context.Banners.Remove(banner);
                _context.SaveChanges();
                var remainingBanners = _context.Banners.OrderBy(b => b.DisplayOrder).ToList();
                for (int i = 0; i < remainingBanners.Count; i++) remainingBanners[i].DisplayOrder = i + 1;
                _context.SaveChanges();
                GhiNhatKy("Xóa Banner", $"Đã xóa banner: {title}");
                TempData["Success"] = "Đã xóa banner.";
            }
            return RedirectToAction("QuanLyBanner");
        }

        // --- 12. QUẢN LÝ VOUCHER ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyVoucher()
        {
            var list = _context.Vouchers.OrderByDescending(v => v.Id).ToList();
            return View(list);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult ThemVoucher()
        {
            return View();
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemVoucher(Voucher model)
        {
            if (_context.Vouchers.Any(v => v.MaVoucher == model.MaVoucher)) ModelState.AddModelError("MaVoucher", "Mã voucher này đã tồn tại!");
            if (ModelState.IsValid)
            {
                model.MaVoucher = model.MaVoucher.ToUpper().Trim();
                _context.Vouchers.Add(model);
                _context.SaveChanges();
                GhiNhatKy("Thêm Voucher", $"Tạo mã: {model.MaVoucher}");
                TempData["Success"] = "Đã tạo mã giảm giá thành công!";
                return RedirectToAction("QuanLyVoucher");
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaVoucher(int id)
        {
            var v = _context.Vouchers.Find(id);
            return v == null ? NotFound() : View(v);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaVoucher(Voucher model)
        {
            if (ModelState.IsValid)
            {
                var v = _context.Vouchers.Find(model.Id);
                if (v != null)
                {
                    v.TenVoucher = model.TenVoucher;
                    v.LoaiGiamGia = model.LoaiGiamGia;
                    v.GiaTri = model.GiaTri;
                    v.GiamToiDa = model.GiamToiDa;
                    v.DonToiThieu = model.DonToiThieu;
                    v.SoLuong = model.SoLuong;
                    v.NgayBatDau = model.NgayBatDau;
                    v.NgayKetThuc = model.NgayKetThuc;
                    v.IsActive = model.IsActive;
                    _context.SaveChanges();
                    GhiNhatKy("Sửa Voucher", $"Cập nhật mã: {v.MaVoucher}");
                    TempData["Success"] = "Cập nhật thành công!";
                    return RedirectToAction("QuanLyVoucher");
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaVoucher(int id)
        {
            var v = _context.Vouchers.Find(id);
            if (v != null)
            {
                _context.Vouchers.Remove(v);
                _context.SaveChanges();
                GhiNhatKy("Xóa Voucher", $"Đã xóa mã: {v.MaVoucher}");
            }
            return RedirectToAction("QuanLyVoucher");
        }

        // --- 13. NHẬT KÝ HOẠT ĐỘNG ---
        [Authorize(Roles = "Boss")]
        public IActionResult NhatKyHoatDong(int? storeId, int page = 1)
        {
            int pageSize = 20;

            var query = _context.LichSuHoatDongs
                                .Include(l => l.TaiKhoan)
                                .AsQueryable();

            query = query.Where(l => l.TaiKhoan.Role != "Boss");

            if (storeId.HasValue)
            {
                query = query.Where(l => l.StoreId == storeId);
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var listLogs = query.OrderByDescending(l => l.ThoiGian)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            ViewBag.SelectedStore = storeId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(listLogs);
        }

        // --- 14. QUẢN LÝ THUẾ ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyThue(int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;
            var query = _context.DonHangs.AsNoTracking().Where(d => d.TrangThai == 3);
            decimal tongThueAll = 0;
            if (query.Any()) tongThueAll = query.Sum(d => d.TienThue);
            var queryMonth = query.Where(d => d.NgayDat.Value.Month == selectedMonth && d.NgayDat.Value.Year == selectedYear);
            decimal thueThang = 0; decimal doanhThuTong = 0;
            if (queryMonth.Any())
            {
                thueThang = queryMonth.Sum(d => d.TienThue);
                doanhThuTong = queryMonth.Sum(d => d.TongTien);
            }
            decimal doanhThuNet = doanhThuTong - thueThang;
            var listDonHangThue = queryMonth.OrderByDescending(d => d.NgayDat).ToList();
            ViewBag.TongThueAll = tongThueAll; ViewBag.ThueThang = thueThang; ViewBag.DoanhThuNet = doanhThuNet;
            ViewBag.SelectedMonth = selectedMonth; ViewBag.SelectedYear = selectedYear;
            return View(listDonHangThue);
        }

        [Authorize(Roles = "Boss")]
        public IActionResult XuatExcelThue(int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;
            var query = _context.DonHangs.AsNoTracking().Where(d => d.TrangThai == 3 && d.NgayDat.Value.Month == selectedMonth && d.NgayDat.Value.Year == selectedYear).OrderByDescending(d => d.NgayDat).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Mã đơn hàng,Ngày hóa đơn,Tổng thanh toán,Thuế VAT (10%),Doanh thu Net");
            foreach (var item in query)
            {
                decimal net = item.TongTien - item.TienThue;
                string ngay = item.NgayDat.Value.ToString("dd/MM/yyyy HH:mm");
                sb.AppendLine($"{item.MaDon},{ngay},{item.TongTien},{item.TienThue},{net}");
            }
            string fileName = $"BaoCaoThue_T{selectedMonth}_{selectedYear}.csv";
            var fileContent = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(fileContent, "text/csv", fileName);
        }

        // --- 16. TÍNH LƯƠNG NHÂN VIÊN ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyLuong(int? month, int? year)
        {
            var now = DateTime.Now;
            int sMonth = month ?? now.Month;
            int sYear = year ?? now.Year;

            var admins = _context.TaiKhoans.Where(u => u.Role == "Admin").ToList();
            var configs = _context.CauHinhLuongs.ToList();
            var paidSalaries = _context.BangLuongs
                .Where(b => b.Thang == sMonth && b.Nam == sYear)
                .ToList();

            var payrollList = new List<PayrollViewModel>();

            foreach (var ad in admins)
            {
                var paid = paidSalaries.FirstOrDefault(p => p.TaiKhoanId == ad.Id);

                if (paid != null)
                {
                    double phanTram = 0;
                    if (paid.DoanhSoDatDuoc > 0)
                    {
                        phanTram = (double)(paid.TienHoaHong / paid.DoanhSoDatDuoc * 100);
                        phanTram = Math.Round(phanTram, 2);
                    }

                    payrollList.Add(new PayrollViewModel
                    {
                        TaiKhoanId = ad.Id,
                        FullName = ad.FullName,
                        Username = ad.Username,
                        StoreId = ad.StoreId,
                        LuongCung = paid.LuongCung,
                        PhanTramHoaHong = phanTram,
                        DoanhSo = paid.DoanhSoDatDuoc,
                        TienHoaHong = paid.TienHoaHong,
                        TongThucNhan = paid.TongThucNhan,
                        DaChot = true,
                        NgayChot = paid.NgayChot
                    });
                }
                else
                {
                    var config = configs.FirstOrDefault(c => c.TaiKhoanId == ad.Id);
                    decimal luongCung = config?.LuongCung ?? 0;
                    double phanTram = config?.PhanTramHoaHong ?? 0;

                    decimal doanhSo = 0;
                    if (ad.StoreId.HasValue)
                    {
                        doanhSo = _context.DonHangs
                            .Where(d => d.StoreId == ad.StoreId
                                     && d.TrangThai == 3
                                     && d.NgayDat.Value.Month == sMonth
                                     && d.NgayDat.Value.Year == sYear)
                            .Sum(d => d.TongTien - d.TienThue);
                    }

                    decimal hoaHong = doanhSo * (decimal)(phanTram / 100);

                    payrollList.Add(new PayrollViewModel
                    {
                        TaiKhoanId = ad.Id,
                        FullName = ad.FullName,
                        Username = ad.Username,
                        StoreId = ad.StoreId,
                        LuongCung = luongCung,
                        PhanTramHoaHong = phanTram,
                        DoanhSo = doanhSo,
                        TienHoaHong = hoaHong,
                        TongThucNhan = luongCung + hoaHong,
                        DaChot = false
                    });
                }
            }

            ViewBag.SelectedMonth = sMonth;
            ViewBag.SelectedYear = sYear;
            return View(payrollList);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult CapNhatCauHinhLuong(int taiKhoanId, decimal luongCung, double phanTram)
        {
            var config = _context.CauHinhLuongs.FirstOrDefault(c => c.TaiKhoanId == taiKhoanId);
            if (config == null)
            {
                config = new CauHinhLuong { TaiKhoanId = taiKhoanId };
                _context.CauHinhLuongs.Add(config);
            }
            config.LuongCung = luongCung;
            config.PhanTramHoaHong = phanTram;
            _context.SaveChanges();

            GhiNhatKy("Cấu hình lương", $"Cập nhật lương ID {taiKhoanId}: Cứng {luongCung:N0}, HH {phanTram}%");
            return Json(new { success = true });
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult ChotLuong(int taiKhoanId, int month, int year)
        {
            if (_context.BangLuongs.Any(b => b.TaiKhoanId == taiKhoanId && b.Thang == month && b.Nam == year))
                return Json(new { success = false, message = "Đã chốt lương nhân viên này rồi!" });

            var user = _context.TaiKhoans.Find(taiKhoanId);
            var config = _context.CauHinhLuongs.FirstOrDefault(c => c.TaiKhoanId == taiKhoanId);

            decimal luongCung = config?.LuongCung ?? 0;
            double phanTram = config?.PhanTramHoaHong ?? 0;

            decimal doanhSo = 0;
            if (user.StoreId.HasValue)
            {
                doanhSo = _context.DonHangs
                    .Where(d => d.StoreId == user.StoreId && d.TrangThai == 3 && d.NgayDat.Value.Month == month && d.NgayDat.Value.Year == year)
                    .Sum(d => d.TongTien - d.TienThue);
            }
            decimal hoaHong = doanhSo * (decimal)(phanTram / 100);

            var bangLuong = new BangLuong
            {
                TaiKhoanId = taiKhoanId,
                Thang = month,
                Nam = year,
                DoanhSoDatDuoc = doanhSo,
                LuongCung = luongCung,
                TienHoaHong = hoaHong,
                TongThucNhan = luongCung + hoaHong,
                NgayChot = DateTime.Now,
                NguoiChot = User.Identity.Name
            };

            _context.BangLuongs.Add(bangLuong);
            _context.SaveChanges();

            GhiNhatKy("Chốt lương", $"Đã chốt lương tháng {month}/{year} cho {user.Username}. Tổng: {bangLuong.TongThucNhan:N0}");
            return Json(new { success = true });
        }

        // --- 17. THU NHẬP CỦA TÔI ---
        public IActionResult LuongCuaToi()
        {
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            var config = _context.CauHinhLuongs.FirstOrDefault(c => c.TaiKhoanId == user.Id);
            decimal luongCung = config?.LuongCung ?? 0;
            double phanTram = config?.PhanTramHoaHong ?? 0;

            var now = DateTime.Now;
            decimal doanhSoThangNay = 0;

            if (user.StoreId.HasValue)
            {
                doanhSoThangNay = _context.DonHangs
                    .Where(d => d.StoreId == user.StoreId
                             && d.TrangThai == 3
                             && d.NgayDat.Value.Month == now.Month
                             && d.NgayDat.Value.Year == now.Year)
                    .Sum(d => d.TongTien - d.TienThue);
            }

            decimal hoaHongDuKien = doanhSoThangNay * (decimal)(phanTram / 100);

            var lichSuLuong = _context.BangLuongs
                .Where(b => b.TaiKhoanId == user.Id)
                .OrderByDescending(b => b.Nam).ThenByDescending(b => b.Thang)
                .ToList();

            ViewBag.ThangNay = now.Month;
            ViewBag.LuongCung = luongCung;
            ViewBag.PhanTram = phanTram;
            ViewBag.DoanhSo = doanhSoThangNay;
            ViewBag.HoaHong = hoaHongDuKien;
            ViewBag.TongDuKien = luongCung + hoaHongDuKien;

            return View(lichSuLuong);
        }
    }
}