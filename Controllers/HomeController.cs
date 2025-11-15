using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TestDoAn.Data;
using TestDoAn.Models;

namespace TestDoAn.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string search, string category)
        {
            var sanpham = _context.SanPhams.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                sanpham = sanpham.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                sanpham = sanpham.Where(p => p.Category == category);
            }
            ViewData["SearchQuery"] = search;
            ViewData["SelectedCategory"] = category;
            ViewData["Categories"] = _context.SanPhams.Select(p => p.Category).Distinct().ToList();
            return View(sanpham.ToList());
        }

        public IActionResult ChiTiet(int id)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}