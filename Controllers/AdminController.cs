using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services; // [MỚI]
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading.Tasks;

namespace banthietbidientu.Controllers
{
    [Authorize(Roles = "Admin,Boss")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; // [MỚI]

        // [CẬP NHẬT] Constructor nhận thêm IEmailSender để gửi mail
        public AdminController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // --- 1. DASHBOARD ---
        public IActionResult Index()
        {
            var user = _context.TaiKhoans.AsNoTracking()
                .FirstOrDefault(u => u.Username == User.Identity.Name);

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

        // [ASYNC] Để gửi mail
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(string orderId, string status)
        {
            try
            {
                // Include TaiKhoan để lấy email khách
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

                // [MỚI] Gửi email khi hoàn thành
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

        // --- 3. QUẢN LÝ NHẬP HÀNG (CHỈ BOSS) ---
        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyNhapHang()
        {
            var listPhieuNhap = _context.PhieuNhaps
                .Include(p => p.ChiTiets)
                    .ThenInclude(ct => ct.SanPham)
                .OrderByDescending(p => p.NgayNhap)
                .ToList();

            return View(listPhieuNhap);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult NhapHang()
        {
            ViewBag.SanPhams = _context.SanPhams.ToList();
            return View();
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XuLyNhapHang(int sanPhamId, int soLuong, decimal giaNhap, string ghiChu)
        {
            var sanPham = _context.SanPhams.Find(sanPhamId);
            if (sanPham == null) return NotFound();

            decimal tongGiaTriCu = sanPham.SoLuong * sanPham.GiaNhap;
            decimal tongGiaTriNhap = soLuong * giaNhap;
            int tongSoLuongMoi = sanPham.SoLuong + soLuong;

            if (tongSoLuongMoi > 0)
            {
                sanPham.GiaNhap = (tongGiaTriCu + tongGiaTriNhap) / tongSoLuongMoi;
            }

            sanPham.SoLuong += soLuong;

            var phieuNhap = new PhieuNhap
            {
                NgayNhap = DateTime.Now,
                GhiChu = string.IsNullOrEmpty(ghiChu) ? "Nhập hàng" : ghiChu,
                TongTien = soLuong * giaNhap
            };
            _context.PhieuNhaps.Add(phieuNhap);
            _context.SaveChanges();

            var chiTiet = new ChiTietPhieuNhap
            {
                PhieuNhapId = phieuNhap.Id,
                SanPhamId = sanPhamId,
                SoLuongNhap = soLuong,
                GiaNhap = giaNhap
            };
            _context.ChiTietPhieuNhaps.Add(chiTiet);
            _context.SaveChanges();

            GhiNhatKy("Nhập kho", $"Nhập thêm {soLuong} sản phẩm: {sanPham.Name}. Giá nhập: {giaNhap:N0}");

            TempData["Success"] = $"Đã nhập kho {soLuong} {sanPham.Name}.";
            return RedirectToAction("QuanLyNhapHang");
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
                    // [SỬA LỖI] Admin con chỉ thấy thông báo của đúng Store mình
                    // Bỏ đoạn "|| t.StoreId == null" đi để họ không thấy thông báo của Boss
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
                        redirectAction = x.RedirectAction
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

        // --- 5. QUẢN LÝ SẢN PHẨM ---
        public IActionResult QuanLySanPham()
        {
            return View(_context.SanPhams.AsNoTracking().ToList());
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult ThemSanPham()
        {
            return View();
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            if (ModelState.IsValid)
            {
                model.SoLuong = slHaNoi + slDaNang + slHCM;
                model.MoTa = $"Sản phẩm {model.Name} chính hãng.||LOC:{slHaNoi}-{slDaNang}-{slHCM}||";
                _context.SanPhams.Add(model);
                _context.SaveChanges();

                GhiNhatKy("Thêm sản phẩm", $"Thêm mới sản phẩm: {model.Name}");
                return RedirectToAction("QuanLySanPham");
            }
            return View(model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();

            int hn = 0, dn = 0, hcm = 0;

            if (!string.IsNullOrEmpty(sp.MoTa) && sp.MoTa.Contains("||LOC:"))
            {
                try
                {
                    var parts = sp.MoTa.Split(new[] { "||LOC:", "||" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (part.Contains("-"))
                        {
                            var nums = part.Split('-');
                            if (nums.Length == 3)
                            {
                                int.TryParse(nums[0], out hn);
                                int.TryParse(nums[1], out dn);
                                int.TryParse(nums[2], out hcm);
                            }
                            break;
                        }
                    }
                }
                catch { }
            }

            ViewBag.SlHaNoi = hn;
            ViewBag.SlDaNang = dn;
            ViewBag.SlHCM = hcm;
            return View(sp);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            var sp = _context.SanPhams.Find(model.Id);
            if (sp != null)
            {
                sp.Name = model.Name;
                sp.Price = model.Price;
                sp.GiaNhap = model.GiaNhap;
                sp.Category = model.Category;
                sp.ImageUrl = model.ImageUrl;
                sp.SoLuong = slHaNoi + slDaNang + slHCM;
                sp.MoTa = $"Sản phẩm {model.Name} chính hãng.||LOC:{slHaNoi}-{slDaNang}-{slHCM}||";
                _context.SaveChanges();
                GhiNhatKy("Sửa sản phẩm", $"Cập nhật thông tin sản phẩm ID: {sp.Id} - {sp.Name}");
            }
            return RedirectToAction("QuanLySanPham");
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public async Task<IActionResult> CapNhatGiaNhapNhanh(int id, decimal giaNhap)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                if (giaNhap < 0) return Json(new { success = false, message = "Lỗi" });
                sp.GiaNhap = giaNhap;
                await _context.SaveChangesAsync();
                GhiNhatKy("Cập nhật giá vốn", $"Cập nhật nhanh giá vốn cho sản phẩm ID {id} thành {giaNhap:N0}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp != null)
            {
                string tenSp = sp.Name;
                _context.SanPhams.Remove(sp);
                _context.SaveChanges();
                GhiNhatKy("Xóa sản phẩm", $"Đã xóa sản phẩm: {tenSp} (ID: {id})");
            }
            return RedirectToAction("QuanLySanPham");
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

        // --- [ĐÃ BỔ SUNG] CÁC HÀM CÒN THIẾU ---

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

            // [QUAN TRỌNG] Thêm Include(l => l.TaiKhoan) để truy cập được cột Role
            var query = _context.LichSuHoatDongs
                                .Include(l => l.TaiKhoan)
                                .AsQueryable();

            // [LOGIC MỚI] Loại bỏ nhật ký của chính Boss
            // Chỉ hiển thị hoạt động của Admin và nhân viên khác
            query = query.Where(l => l.TaiKhoan.Role != "Boss");

            if (storeId.HasValue)
            {
                query = query.Where(l => l.StoreId == storeId);
            }

            var listLogs = query.OrderByDescending(l => l.ThoiGian)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            ViewBag.SelectedStore = storeId;
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

        // --- 15. QUẢN LÝ LUÂN CHUYỂN HÀNG HÓA (KHO VẬN) ---

        public IActionResult QuanLyChuyenKho()
        {
            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
            int? storeId = user?.StoreId;
            bool isBoss = user?.Role == "Boss";

            var query = _context.PhieuChuyenKhos.Include(p => p.SanPham).AsQueryable();

            if (!isBoss && storeId.HasValue)
            {
                // Admin chỉ thấy phiếu liên quan đến mình
                query = query.Where(p => p.TuKhoId == storeId || p.DenKhoId == storeId);
            }

            return View(query.OrderByDescending(p => p.NgayTao).ToList());
        }

        [HttpGet]
        public IActionResult TaoLenhChuyenKho()
        {
            ViewBag.SanPhams = _context.SanPhams.Where(p => p.SoLuong > 0).ToList();
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);

            // Truyền Role và StoreId sang View để ẩn/hiện ô nhập
            ViewBag.CurrentStoreId = user?.StoreId;
            ViewBag.IsBoss = user?.Role == "Boss";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TaoLenhChuyenKho(PhieuChuyenKho model)
        {
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");
            bool isBoss = user.Role == "Boss";

            var sp = _context.SanPhams.Find(model.SanPhamId);

            // --- LOGIC PHÂN QUYỀN ---
            if (!isBoss)
            {
                // Admin con: Bắt buộc Kho Đích là chính mình, Kho Nguồn = 0 (Chờ Boss chọn)
                model.DenKhoId = user.StoreId ?? 0;
                model.TuKhoId = 0; // 0 nghĩa là chưa xác định nguồn
            }
            else
            {
                // Boss: Phải chọn đủ 2 kho
                if (model.TuKhoId == 0 || model.DenKhoId == 0)
                {
                    ModelState.AddModelError("", "Vui lòng chọn đủ Kho đi và Kho đến!");
                }
            }

            if (model.TuKhoId != 0 && model.TuKhoId == model.DenKhoId)
            {
                ModelState.AddModelError("", "Kho đi và Kho đến không được trùng nhau!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.SanPhams = _context.SanPhams.Where(p => p.SoLuong > 0).ToList();
                ViewBag.CurrentStoreId = user.StoreId;
                ViewBag.IsBoss = isBoss;
                return View(model);
            }

            // Tạo phiếu
            model.MaPhieu = "CK" + DateTime.Now.ToString("yyyyMMddHHmm");
            model.NguoiTao = user.Username;
            model.NgayTao = DateTime.Now;
            model.TrangThai = 0; // Mới tạo
            if (string.IsNullOrEmpty(model.GhiChu)) model.GhiChu = "";

            _context.PhieuChuyenKhos.Add(model);
            await _context.SaveChangesAsync();

            // --- THÔNG BÁO THÔNG MINH ---
            if (!isBoss)
            {
                // Admin tạo -> Báo cho Boss duyệt
                TaoThongBaoHeThong("Yêu cầu điều hàng",
                    $"Admin kho {GetStoreName(model.DenKhoId)} xin cấp {model.SoLuong} {sp.Name}. Vui lòng chọn kho nguồn cấp hàng.",
                    model.Id.ToString(), "QuanLyChuyenKho", null); // Null = Gửi Boss
            }
            else
            {
                // Boss tạo -> Báo cho Kho Nguồn chuẩn bị xuất
                TaoThongBaoHeThong("Lệnh điều phối từ Boss",
                    $"Boss yêu cầu chuyển {model.SoLuong} {sp.Name} sang Kho {GetStoreName(model.DenKhoId)}. Vui lòng xuất kho.",
                    model.Id.ToString(), "QuanLyChuyenKho", model.TuKhoId);
            }

            TempData["Success"] = "Đã tạo lệnh chuyển kho thành công!";
            return RedirectToAction("QuanLyChuyenKho");
        }

        // [MỚI] Hàm cho Boss duyệt và chọn nguồn cấp cho các phiếu Admin tạo
        [HttpPost]
        public async Task<IActionResult> DuyetYeuCau(int id, int tuKhoId)
        {
            if (!User.IsInRole("Boss")) return Json(new { success = false, message = "Chỉ Boss mới được duyệt!" });

            var phieu = _context.PhieuChuyenKhos.Include(p => p.SanPham).FirstOrDefault(p => p.Id == id);
            if (phieu == null) return Json(new { success = false, message = "Phiếu không tồn tại" });

            if (tuKhoId == phieu.DenKhoId) return Json(new { success = false, message = "Kho nguồn trùng kho đích!" });

            // Cập nhật Kho Nguồn
            phieu.TuKhoId = tuKhoId;
            await _context.SaveChangesAsync();

            // Báo cho Kho Nguồn biết để xuất hàng
            TaoThongBaoHeThong("Lệnh xuất hàng",
                $"Boss đã duyệt yêu cầu #{phieu.MaPhieu}. Vui lòng chuyển hàng đi Kho {GetStoreName(phieu.DenKhoId)}.",
                phieu.Id.ToString(), "QuanLyChuyenKho", tuKhoId);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DuyetXuatKho(int id)
        {
            var phieu = _context.PhieuChuyenKhos.Include(p => p.SanPham).FirstOrDefault(p => p.Id == id);
            if (phieu == null || phieu.TrangThai != 0) return Json(new { success = false, message = "Phiếu không hợp lệ" });

            if (phieu.TuKhoId == 0) return Json(new { success = false, message = "Chưa chọn kho nguồn! Boss cần duyệt trước." });

            bool result = UpdateStockDetail(phieu.SanPham, phieu.TuKhoId, -phieu.SoLuong);
            if (!result) return Json(new { success = false, message = "Kho đi không đủ hàng thực tế!" });

            phieu.TrangThai = 1; // Đang chuyển
            await _context.SaveChangesAsync();

            // Báo cho Kho Đích chuẩn bị nhận
            TaoThongBaoHeThong("Hàng đang đến",
                $"Kho {GetStoreName(phieu.TuKhoId)} đã xuất hàng theo phiếu #{phieu.MaPhieu}. Chuẩn bị nhận hàng.",
                phieu.Id.ToString(), "QuanLyChuyenKho", phieu.DenKhoId);

            GhiNhatKy("Xuất kho", $"Phiếu {phieu.MaPhieu}: Xuất {phieu.SoLuong} {phieu.SanPham.Name} từ {GetStoreName(phieu.TuKhoId)}");
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanNhanHang(int id)
        {
            var phieu = _context.PhieuChuyenKhos.Include(p => p.SanPham).FirstOrDefault(p => p.Id == id);
            if (phieu == null || phieu.TrangThai != 1) return Json(new { success = false });

            UpdateStockDetail(phieu.SanPham, phieu.DenKhoId, phieu.SoLuong);
            phieu.TrangThai = 2; // Hoàn tất
            phieu.NgayHoanThanh = DateTime.Now;
            await _context.SaveChangesAsync();

            // [THÔNG BÁO RIÊNG BIỆT]
            // 1. Báo cáo Boss
            TaoThongBaoHeThong("Hoàn tất điều phối",
                $"Phiếu {phieu.MaPhieu} hoàn tất. Kho {GetStoreName(phieu.DenKhoId)} đã nhận đủ hàng.",
                phieu.Id.ToString(), "QuanLyChuyenKho", null);

            // 2. Báo người gửi (Kho Nguồn)
            TaoThongBaoHeThong("Đã nhận hàng",
                $"Kho {GetStoreName(phieu.DenKhoId)} xác nhận đã nhận lô hàng bạn gửi (Phiếu {phieu.MaPhieu}).",
                phieu.Id.ToString(), "QuanLyChuyenKho", phieu.TuKhoId);

            GhiNhatKy("Nhập kho", $"Phiếu {phieu.MaPhieu}: Nhập {phieu.SoLuong} {phieu.SanPham.Name} vào {GetStoreName(phieu.DenKhoId)}");
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult HuyChuyenKho(int id)
        {
            var phieu = _context.PhieuChuyenKhos.Find(id);
            if (phieu != null && phieu.TrangThai == 0)
            {
                phieu.TrangThai = -1;
                _context.SaveChanges();
                GhiNhatKy("Hủy chuyển kho", $"Đã hủy phiếu {phieu.MaPhieu}");
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không thể hủy phiếu này" });
        }

        private void GhiNhatKy(string hanhDong, string noiDung)
        {
            try
            {
                var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    var log = new LichSuHoatDong { TaiKhoanId = user.Id, TenNguoiDung = user.FullName ?? user.Username, StoreId = user.StoreId, HanhDong = hanhDong, NoiDung = noiDung, ThoiGian = DateTime.Now };
                    _context.LichSuHoatDongs.Add(log);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi ghi nhật ký: " + ex.Message); }
        }

        // Helper lấy tên kho cho thông báo sinh động
        private string GetStoreName(int storeId)
        {
            return storeId switch { 1 => "Hà Nội", 2 => "Đà Nẵng", 3 => "TP.HCM", _ => "Kho ?" };
        }

        // --- CÁC HÀM BỔ TRỢ XỬ LÝ CHUỖI LOC ---

        private int GetStockByStore(SanPham sp, int storeId)
        {
            if (string.IsNullOrEmpty(sp.MoTa) || !sp.MoTa.Contains("||LOC:")) return 0;
            try
            {
                var parts = sp.MoTa.Split(new[] { "||LOC:", "||" }, StringSplitOptions.RemoveEmptyEntries);
                string locPart = parts.FirstOrDefault(p => p.Contains("-") && p.Any(char.IsDigit));
                if (locPart != null)
                {
                    var nums = locPart.Split('-').Select(int.Parse).ToArray();
                    int index = storeId - 1; // Store 1->0, 2->1, 3->2
                    if (index >= 0 && index < nums.Length) return nums[index];
                }
            }
            catch { }
            return 0;
        }

        private bool UpdateStockDetail(SanPham sp, int storeId, int quantityChange)
        {
            try
            {
                // 1. Cập nhật Tổng số lượng
                sp.SoLuong += quantityChange;

                // 2. Cập nhật Chi tiết
                if (!string.IsNullOrEmpty(sp.MoTa) && sp.MoTa.Contains("||LOC:"))
                {
                    var parts = sp.MoTa.Split(new[] { "||LOC:", "||" }, StringSplitOptions.RemoveEmptyEntries);
                    string locPart = parts.FirstOrDefault(p => p.Contains("-") && p.Any(char.IsDigit));

                    if (locPart != null)
                    {
                        var nums = locPart.Split('-').Select(int.Parse).ToArray();
                        int index = storeId - 1;

                        if (index >= 0 && index < nums.Length)
                        {
                            // Kiểm tra nếu là phép trừ mà không đủ hàng
                            if (quantityChange < 0 && nums[index] < Math.Abs(quantityChange)) return false;

                            nums[index] += quantityChange;
                            string newLoc = $"{nums[0]}-{nums[1]}-{nums[2]}";
                            sp.MoTa = sp.MoTa.Replace(locPart, newLoc);
                            return true;
                        }
                    }
                }
                // Nếu chưa có chuỗi LOC thì coi như lỗi cấu trúc (hoặc có thể init mới nếu muốn)
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void TaoThongBaoHeThong(string tieuDe, string noiDung, string redirectId, string action, int? storeId)
        {
            var tb = new ThongBao
            {
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 4, // Loại 4: Chuyển kho
                RedirectId = redirectId,
                RedirectAction = action,
                StoreId = storeId
            };
            _context.ThongBaos.Add(tb);
            _context.SaveChanges();
        }

        // --- 16. [MỚI] TÍNH LƯƠNG NHÂN VIÊN (PAYROLL) ---

        [Authorize(Roles = "Boss")]
        public IActionResult QuanLyLuong(int? month, int? year)
        {
            var now = DateTime.Now;
            int sMonth = month ?? now.Month;
            int sYear = year ?? now.Year;

            // 1. Lấy danh sách Admin chi nhánh
            var admins = _context.TaiKhoans.Where(u => u.Role == "Admin").ToList();

            // 2. Lấy cấu hình lương & Lịch sử lương đã trả
            var configs = _context.CauHinhLuongs.ToList();
            var paidSalaries = _context.BangLuongs
                .Where(b => b.Thang == sMonth && b.Nam == sYear)
                .ToList();

            var payrollList = new List<PayrollViewModel>();

            foreach (var ad in admins)
            {
                var paid = paidSalaries.FirstOrDefault(p => p.TaiKhoanId == ad.Id);

                if (paid != null) // Đã chốt lương -> Lấy từ lịch sử
                {
                    payrollList.Add(new PayrollViewModel
                    {
                        TaiKhoanId = ad.Id,
                        FullName = ad.FullName,
                        Username = ad.Username,
                        StoreId = ad.StoreId,
                        LuongCung = paid.LuongCung,
                        PhanTramHoaHong = (double)(paid.DoanhSoDatDuoc > 0 ? (paid.TienHoaHong / paid.DoanhSoDatDuoc * 100) : 0),
                        DoanhSo = paid.DoanhSoDatDuoc,
                        TienHoaHong = paid.TienHoaHong,
                        TongThucNhan = paid.TongThucNhan,
                        DaChot = true,
                        NgayChot = paid.NgayChot
                    });
                }
                else // Chưa chốt -> Tính toán Real-time
                {
                    var config = configs.FirstOrDefault(c => c.TaiKhoanId == ad.Id);
                    decimal luongCung = config?.LuongCung ?? 0;
                    double phanTram = config?.PhanTramHoaHong ?? 0;

                    // Tính doanh số thuần (trừ thuế) của Store đó trong tháng
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
            // Kiểm tra đã chốt chưa
            if (_context.BangLuongs.Any(b => b.TaiKhoanId == taiKhoanId && b.Thang == month && b.Nam == year))
                return Json(new { success = false, message = "Đã chốt lương nhân viên này rồi!" });

            // Lấy lại dữ liệu để chốt (Không tin dữ liệu từ client gửi lên)
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
    }
}