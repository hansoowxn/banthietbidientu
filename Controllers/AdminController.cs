using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
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

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. DASHBOARD (ĐÃ NÂNG CẤP TÍNH TĂNG TRƯỞNG) ---
        public IActionResult Index()
        {
            // 1. Lấy User & StoreId
            var user = _context.TaiKhoans.AsNoTracking()
                .FirstOrDefault(u => u.Username == User.Identity.Name);

            int? storeId = user?.StoreId;

            var queryDonHang = _context.DonHangs.AsQueryable();

            // Phân quyền: Lọc theo Store nếu có
            if (storeId.HasValue)
            {
                queryDonHang = queryDonHang.Where(d => d.StoreId == storeId);
            }

            var today = DateTime.Today;

            // 2. KPI Hôm nay
            ViewBag.DoanhThuHomNay = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Date == today)
                .Sum(x => x.TongTien);

            ViewBag.DonHangHomNay = queryDonHang
                .Count(d => d.NgayDat.Value.Date == today);

            ViewBag.DonChoXuLy = queryDonHang
                .Count(d => d.TrangThai == 0 || d.TrangThai == 1);

            // 3. [MỚI] KPI THÁNG NÀY vs THÁNG TRƯỚC
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Doanh thu tháng này
            decimal revThisMonth = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat >= startOfMonth)
                .Sum(x => x.TongTien);

            // Doanh thu tháng trước
            decimal revLastMonth = queryDonHang
                .Where(d => d.TrangThai == 3 && d.NgayDat >= startOfLastMonth && d.NgayDat <= endOfLastMonth)
                .Sum(x => x.TongTien);

            // Tính % Tăng trưởng
            double growth = 0;
            if (revLastMonth > 0)
            {
                growth = (double)((revThisMonth - revLastMonth) / revLastMonth) * 100;
            }
            else if (revThisMonth > 0)
            {
                growth = 100; // Tháng trước = 0 mà tháng này có tiền -> Tăng trưởng tuyệt đối
            }

            ViewBag.RevenueThisMonth = revThisMonth;
            ViewBag.RevenueGrowth = growth;

            // Giữ lại Tổng Doanh Thu Toàn Thời Gian (để dùng nếu cần)
            ViewBag.TongDoanhThu = queryDonHang.Where(d => d.TrangThai == 3).Sum(x => x.TongTien);

            // Các chỉ số khác
            ViewBag.TongDonHang = queryDonHang.Count();
            ViewBag.TongKhachHang = _context.TaiKhoans.Count(x => x.Role == "User");

            // 4. Biểu đồ 7 ngày
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

            // 5. Biểu đồ Tròn
            ViewBag.PieCompleted = queryDonHang.Count(x => x.TrangThai == 3);
            ViewBag.PieShipping = queryDonHang.Count(x => x.TrangThai == 2);
            ViewBag.PiePending = queryDonHang.Count(x => x.TrangThai == 0 || x.TrangThai == 1);
            ViewBag.PieCancelled = queryDonHang.Count(x => x.TrangThai == -1);

            // 6. Danh sách đơn mới
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
        public IActionResult CapNhatTrangThai(string orderId, string status)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDon == orderId);
                if (donHang == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy đơn {orderId}" });
                }

                int statusInt = 0;
                if (status == "Đã xác nhận") statusInt = 1;
                else if (status == "Đang giao") statusInt = 2;
                else if (status == "Hoàn thành") statusInt = 3;
                else if (status == "Đã hủy") statusInt = -1;

                donHang.TrangThai = statusInt;
                _context.SaveChanges();

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

            TempData["Success"] = $"Đã nhập kho {soLuong} {sanPham.Name}.";
            return RedirectToAction("QuanLyNhapHang");
        }

        // --- 4. API THÔNG BÁO ---
        [HttpGet]
        public IActionResult GetThongBao()
        {
            try
            {
                // 1. Lấy thông tin User hiện tại
                var user = _context.TaiKhoans.AsNoTracking()
                    .FirstOrDefault(u => u.Username == User.Identity.Name);

                int? storeId = user?.StoreId;
                bool isBoss = user?.Role == "Boss";

                // 2. Tạo Query cơ bản
                var query = _context.ThongBaos.AsQueryable();

                // 3. Lọc dữ liệu
                if (!isBoss && storeId.HasValue)
                {
                    // Admin chi nhánh: Chỉ xem tin của Store mình HOẶC tin chung (Null)
                    query = query.Where(t => t.StoreId == storeId || t.StoreId == null);
                }
                // Boss: Xem hết (Không cần lọc)

                // 4. Lấy dữ liệu ra
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
            foreach (var item in list)
            {
                item.DaDoc = true;
            }
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
                _context.SanPhams.Remove(sp);
                _context.SaveChanges();
            }
            return RedirectToAction("QuanLySanPham");
        }

        // --- 6. QUẢN LÝ TÀI KHOẢN (CHỈ BOSS) ---
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
                _context.TaiKhoans.Remove(tk);
                _context.SaveChanges();
            }
            return RedirectToAction("QuanLyTaiKhoan");
        }

        // --- 7. BÁO CÁO NÂNG CAO (CHỈ BOSS) ---
        [Authorize(Roles = "Boss")]
        public IActionResult BaoCao(int? storeId, int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;
            int prevMonth = selectedMonth == 1 ? 12 : selectedMonth - 1;
            int prevYearOfMonth = selectedMonth == 1 ? selectedYear - 1 : selectedYear;

            var user = _context.TaiKhoans.AsNoTracking()
                .FirstOrDefault(u => u.Username == User.Identity.Name);

            // Mặc định tên Store
            string storeName = "Toàn hệ thống";
            if (storeId == 1) storeName = "Chi nhánh Hà Nội";
            else if (storeId == 2) storeName = "Chi nhánh Đà Nẵng";
            else if (storeId == 3) storeName = "Chi nhánh TP.HCM";

            // Hàm local lấy dữ liệu tháng
            List<decimal> GetMonthlyData(int y)
            {
                var data = new List<decimal>();
                for (int m = 1; m <= 12; m++)
                {
                    var revenue = _context.DonHangs
                        .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == y
                                    && d.NgayDat.Value.Month == m
                                    && (!storeId.HasValue || d.StoreId == storeId))
                        .Sum(d => (decimal?)d.TongTien) ?? 0;
                    data.Add(revenue);
                }
                return data;
            }

            double CalcGrowth(decimal cur, decimal prev) => prev > 0 ? (double)((cur - prev) / prev) * 100 : 100;

            (decimal DoanhThu, int DonHang, int SanPham, decimal LoiNhuan) CalculateKpi(int m, int y)
            {
                var queryDH = _context.DonHangs
                    .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == y
                                && d.NgayDat.Value.Month == m
                                && (!storeId.HasValue || d.StoreId == storeId));

                var queryCT = _context.ChiTietDonHangs
                    .Where(ct => ct.DonHang.TrangThai == 3
                                 && ct.DonHang.NgayDat.Value.Year == y
                                 && ct.DonHang.NgayDat.Value.Month == m
                                 && (!storeId.HasValue || ct.DonHang.StoreId == storeId));

                decimal dt = queryDH.Sum(d => d.TongTien);
                decimal von = queryCT.Sum(ct => ct.GiaGoc * ct.SoLuong);

                return (dt, queryDH.Count(), queryCT.Sum(ct => ct.SoLuong), dt - von);
            }

            var current = CalculateKpi(selectedMonth, selectedYear);
            var previous = CalculateKpi(prevMonth, prevYearOfMonth);

            var categoryData = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang.TrangThai == 3
                             && ct.DonHang.NgayDat.Value.Year == selectedYear
                             && ct.DonHang.NgayDat.Value.Month == selectedMonth
                             && (!storeId.HasValue || ct.DonHang.StoreId == storeId))
                .GroupBy(ct => ct.SanPham.Category)
                .Select(g => new { Name = g.Key, Value = g.Sum(x => (decimal?)x.Gia * x.SoLuong) ?? 0 })
                .OrderByDescending(x => x.Value)
                .ToList();

            var storeComparison = new List<decimal>();
            if (!storeId.HasValue)
            {
                for (int i = 1; i <= 3; i++)
                {
                    storeComparison.Add(_context.DonHangs
                        .Where(d => d.TrangThai == 3
                                    && d.NgayDat.Value.Year == selectedYear
                                    && d.NgayDat.Value.Month == selectedMonth
                                    && d.StoreId == i)
                        .Sum(d => (decimal?)d.TongTien) ?? 0);
                }
            }

            var queryChiTietFull = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang.TrangThai == 3
                             && ct.DonHang.NgayDat.Value.Year == selectedYear
                             && ct.DonHang.NgayDat.Value.Month == selectedMonth
                             && (!storeId.HasValue || ct.DonHang.StoreId == storeId));

            decimal tongDoanhThuNiemYet = queryChiTietFull.Sum(ct => (ct.Gia ?? 0) * ct.SoLuong);
            decimal tyLeThucThu = tongDoanhThuNiemYet > 0 ? (current.DoanhThu / tongDoanhThuNiemYet) : 1;

            var baoCaoLoiNhuan = queryChiTietFull
                .GroupBy(ct => new { ct.SanPhamId, ct.SanPham.Name, ct.SanPham.ImageUrl })
                .Select(g => new
                {
                    Ten = g.Key.Name,
                    Hinh = g.Key.ImageUrl,
                    Sl = g.Sum(x => x.SoLuong),
                    DtNiemYet = g.Sum(x => (x.Gia ?? 0) * x.SoLuong),
                    Gv = g.Sum(x => x.GiaGoc * x.SoLuong)
                })
                .ToList()
                .Select(x => new LoiNhuanSanPham
                {
                    TenSanPham = x.Ten,
                    HinhAnh = x.Hinh,
                    SoLuongBan = x.Sl,
                    DoanhThuNiemYet = x.DtNiemYet,
                    GiaVon = x.Gv,
                    DoanhThuThuc = x.DtNiemYet * tyLeThucThu,
                    LoiNhuan = (x.DtNiemYet * tyLeThucThu) - x.Gv
                })
                .OrderByDescending(x => x.DoanhThuThuc)
                .ToList();

            var vipUsers = _context.TaiKhoans
                .Where(u => u.Role == "User")
                .Select(u => new
                {
                    User = u,
                    TotalSpent = u.DonHangs.Where(d => d.TrangThai == 3).Sum(d => (decimal?)d.TongTien) ?? 0,
                    OrderCount = u.DonHangs.Count(d => d.TrangThai == 3)
                })
                .Where(x => x.TotalSpent >= 100000000)
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .Select(x => new KhachHangTiemNang
                {
                    Id = x.User.Id,
                    HoTen = x.User.FullName,
                    Username = x.User.Username,
                    SoDienThoai = x.User.PhoneNumber,
                    TongChiTieu = x.TotalSpent,
                    SoDonHang = x.OrderCount
                })
                .ToList();

            var model = new BaoCaoViewModel
            {
                TongDoanhThu = current.DoanhThu,
                TongDoanhThuGoc = tongDoanhThuNiemYet,
                TongDonHang = current.DonHang,
                TongSanPhamDaBan = current.SanPham,
                LoiNhuanUocTinh = current.LoiNhuan,
                SanPhamSapHet = _context.SanPhams.Count(s => s.SoLuong < 5),
                GrowthDoanhThu = CalcGrowth(current.DoanhThu, previous.DoanhThu),
                GrowthLoiNhuan = CalcGrowth(current.LoiNhuan, previous.LoiNhuan),
                GrowthDonHang = CalcGrowth(current.DonHang, previous.DonHang),
                GrowthSanPham = CalcGrowth(current.SanPham, previous.SanPham),
                DataNamHienTai = GetMonthlyData(selectedYear),
                DataNamTruoc = GetMonthlyData(selectedYear - 1),
                DataNamKia = GetMonthlyData(selectedYear - 2),
                CurrentYear = selectedYear,
                CategoryLabels = categoryData.Select(x => x.Name).ToList(),
                CategoryValues = categoryData.Select(x => x.Value).ToList(),
                StoreRevenueComparison = storeComparison,
                SelectedStoreId = storeId,
                StoreName = storeName,
                BaoCaoLoiNhuan = baoCaoLoiNhuan,
                KhachHangTiemNangs = vipUsers
            };

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.IsBoss = true;

            return View(model);
        }

        // --- 8. XUẤT EXCEL & IN HÓA ĐƠN ---
        [HttpGet]
        public IActionResult XuatExcelDonHang()
        {
            var orders = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Mã đơn hàng,Ngày đặt,Khách hàng,Số điện thoại,Địa chỉ giao hàng,Tổng tiền,Trạng thái");

            foreach (var item in orders)
            {
                string trangThaiText = item.TrangThai switch
                {
                    0 => "Chờ xử lý",
                    1 => "Đã xác nhận",
                    2 => "Đang giao",
                    3 => "Hoàn thành",
                    -1 => "Đã hủy",
                    _ => "Khác"
                };
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
            var order = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(d => d.TaiKhoan)
                .FirstOrDefault(d => d.MaDon == orderId);

            if (order == null) return NotFound();

            string shopAddress = "120 Xuân Thủy, Cầu Giấy, Hà Nội";
            string zoneCode = "HN-CG-01";
            string addr = (order.DiaChi ?? "").ToLower();

            string[] mienTrung = { "đà nẵng", "huế", "quảng nam", "quảng ngãi", "bình định", "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa" };
            string[] mienNam = { "hồ chí minh", "tp.hcm", "hcm", "sài gòn", "bình dương", "đồng nai", "bà rịa", "vũng tàu", "long an", "tiền giang", "cần thơ" };

            if (mienNam.Any(k => addr.Contains(k))) { shopAddress = "55 Nguyễn Huệ, Quận 1, TP.HCM"; zoneCode = "SG-Q1-03"; }
            else if (mienTrung.Any(k => addr.Contains(k))) { shopAddress = "78 Bạch Đằng, Hải Châu, Đà Nẵng"; zoneCode = "DN-HC-02"; }

            ViewBag.ShopAddress = shopAddress;
            ViewBag.ZoneCode = zoneCode;

            var model = new DonHangViewModel
            {
                MaDon = order.MaDon,
                NgayDat = order.NgayDat,
                TenKhachHang = !string.IsNullOrEmpty(order.NguoiNhan) ? order.NguoiNhan : (order.TaiKhoan?.FullName ?? "Khách lẻ"),
                SoDienThoai = !string.IsNullOrEmpty(order.SDT) ? order.SDT : (order.TaiKhoan?.PhoneNumber ?? ""),
                DiaChiGiaoHang = order.DiaChi ?? "",
                TongTien = order.TongTien,
                PhuongThucThanhToan = "Thanh toán khi nhận hàng (COD)",
                SanPhams = order.ChiTietDonHangs.Select(ct => new DonHangViewModel
                {
                    TenSanPham = ct.SanPham.Name,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia
                }).ToList()
            };
            return View(model);
        }

        // --- CÁC CHỨC NĂNG KHÁC ---
        public IActionResult QuanLyDanhGia()
        {
            var listDanhGia = _context.DanhGias
                .Include(d => d.SanPham)
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayTao)
                .ToList();
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
            }
            return RedirectToAction("QuanLyDanhGia");
        }

        public IActionResult QuanLyThuMua()
        {
            var listYeuCau = _context.YeuCauThuMuas
                .Include(y => y.TaiKhoan)
                .OrderByDescending(y => y.NgayTao)
                .ToList();
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
            }
            return RedirectToAction("QuanLyThuMua");
        }

        // --- 11. QUẢN LÝ BANNER (CHỈ BOSS) ---
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
            if (ModelState.IsValid)
            {
                _context.Banners.Add(model);
                _context.SaveChanges();
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
            return View(banner);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaBanner(Banner model)
        {
            if (ModelState.IsValid)
            {
                _context.Banners.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật banner thành công!";
                return RedirectToAction("QuanLyBanner");
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
                _context.Banners.Remove(banner);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa banner.";
            }
            return RedirectToAction("QuanLyBanner");
        }
    }
}