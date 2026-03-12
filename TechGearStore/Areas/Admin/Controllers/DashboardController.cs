using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechGearStore.Data;
using TechGearStore.Models; // giả sử bạn có model Order, Product...

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Thêm bảo mật - chỉ Admin mới vào được
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string period = "month") // period: today, week, month, year
        {
            // Lấy ngày hiện tại (VN timezone)
            var now = DateTime.Now;
            DateTime startDate;

            switch (period.ToLower())
            {
                case "today":
                    startDate = now.Date;
                    break;
                case "week":
                    startDate = now.AddDays(-(int)now.DayOfWeek).Date; // đầu tuần
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;
                case "month":
                default:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }

            // Thống kê cơ bản
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalContacts = await _context.ContactInfos.CountAsync();

            // Số liệu nâng cao
            var ordersInPeriod = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= now)
                .AsNoTracking();

            ViewBag.TotalRevenue = await ordersInPeriod
                .SumAsync(o => o.TotalAmount); // giả sử Order có TotalAmount (decimal)

            ViewBag.PendingOrders = await ordersInPeriod
                .CountAsync(o => o.Status == "Pending" || o.Status == "Chờ xác nhận");

            ViewBag.LowStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity <= 10 && p.StockQuantity > 0);

            // Dữ liệu cho Chart (doanh thu theo ngày/tháng)
            var revenueData = await ordersInPeriod
                .GroupBy(o => o.OrderDate.Date) // hoặc .Month nếu period=month/year
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.ChartLabels = revenueData.Select(x => x.Date.ToString("dd/MM")).ToArray();
            ViewBag.ChartData = revenueData.Select(x => x.Revenue).ToArray();

            ViewBag.Period = period; // để highlight nút lọc

            return View();
        }
    }
}