using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechGearStore.Data;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminOrders
        public IActionResult Index()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Admin/AdminOrders/Details/5
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = _context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.OrderId == id)
                .ToList();

            ViewBag.Order = order;

            return View(orderDetails);
        }
    }
}