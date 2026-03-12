using Microsoft.AspNetCore.Mvc;
using TechGearStore.Data;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Products = _context.Products.Count();
            ViewBag.Categories = _context.Categories.Count();
            ViewBag.Users = _context.Users.Count();
            ViewBag.Orders = _context.Orders.Count();
            ViewBag.Contacts = _context.ContactInfos.Count();

            return View();
        }
    }
}