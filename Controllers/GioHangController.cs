using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using TestDoAn.Data;
using TestDoAn.Models;

namespace TestDoAn.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GioHangController> _logger;

        public GioHangController(ApplicationDbContext context, ILogger<GioHangController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        private List<GioHang> GetCart()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
                {
                    return _context.GioHangs
                        .Include(g => g.SanPham)
                        .Where(g => g.UserId == userId)
                        .ToList();
                }
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new List<GioHang>();
            return JsonConvert.DeserializeObject<List<GioHang>>(cartJson) ?? new List<GioHang>();
        }

        private void SaveCart(List<GioHang> cart)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
                {
                    var existingItems = _context.GioHangs.Where(g => g.UserId == userId).ToList();
                    _context.GioHangs.RemoveRange(existingItems);

                    foreach (var item in cart)
                    {
                        var newItem = new GioHang
                        {
                            ProductId = item.ProductId,
                            Name = item.Name,
                            ImageUrl = item.ImageUrl,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            UserId = userId
                        };
                        _context.GioHangs.Add(newItem);
                    }
                    _context.SaveChanges();
                }
            }
            else
            {
                HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
            }
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult Add(int id, string returnToCart = null)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new GioHang
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = 1,
                    UserId = User.Identity.IsAuthenticated && int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null
                });
            }
            SaveCart(cart);

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";
            if (!string.IsNullOrEmpty(returnToCart))
            {
                return RedirectToAction("Index");
            }
            var referrer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referrer))
            {
                return Redirect(referrer);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ThanhToan()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("DangNhap", "Login");
            }

            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index");
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                    return RedirectToAction("DangNhap", "Login");
                }

                var model = new ThanhToanViewModel
                {
                    GioHangs = cart,
                    TaiKhoan = user
                };

                return View(model);
            }

            TempData["Error"] = "Lỗi xác thực người dùng.";
            return RedirectToAction("DangNhap", "Login");
        }

        [HttpPost]
        public IActionResult XacNhanThanhToan(string paymentMethod)
        {
            var cart = GetCart();
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in cart)
                        {
                            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == item.ProductId);
                            if (sanPham == null)
                            {
                                _logger.LogWarning($"Sản phẩm {item.Name} (ID: {item.ProductId}) không tồn tại.");
                                TempData["Error"] = $"Sản phẩm {item.Name} không tồn tại.";
                                return RedirectToAction("ThanhToan");
                            }
                            if (sanPham.SoLuong < item.Quantity)
                            {
                                _logger.LogWarning($"Không đủ số lượng cho sản phẩm {item.Name}. Yêu cầu: {item.Quantity}, Còn: {sanPham.SoLuong}");
                                TempData["Error"] = $"Không đủ số lượng cho sản phẩm {item.Name}.";
                                return RedirectToAction("ThanhToan");
                            }

                            _logger.LogInformation($"Trừ số lượng sản phẩm {item.Name} (ID: {item.ProductId}). Từ {sanPham.SoLuong} thành {sanPham.SoLuong - item.Quantity}");
                            sanPham.SoLuong -= item.Quantity;
                            var lichSu = new LichSuMuaHang
                            {
                                UserId = userId,
                                ProductId = item.ProductId,
                                Name = item.Name,
                                ImageUrl = item.ImageUrl,
                                Price = item.Price,
                                Quantity = item.Quantity,
                                PurchaseDate = DateTime.Now,
                                PaymentMethod = paymentMethod
                            };
                            _context.LichSuMuaHangs.Add(lichSu);
                        }

                        _context.GioHangs.RemoveRange(cart);
                        _context.SaveChanges();
                        transaction.Commit();
                        _logger.LogInformation($"Thanh toán thành công cho UserId: {userId}.");
                        TempData["Success"] = "Thanh toán thành công! Cảm ơn bạn đã mua sắm.";
                        return RedirectToAction("Index", "Home");
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu.");
                        TempData["Error"] = $"Lỗi khi thanh toán: {ex.InnerException?.Message ?? ex.Message}";
                        return RedirectToAction("ThanhToan");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Lỗi không xác định khi thanh toán.");
                        TempData["Error"] = $"Lỗi khi thanh toán: {ex.Message}";
                        return RedirectToAction("ThanhToan");
                    }
                }
            }
            TempData["Error"] = "Lỗi xác thực người dùng.";
            return RedirectToAction("DangNhap", "Login");
        }
    }
}