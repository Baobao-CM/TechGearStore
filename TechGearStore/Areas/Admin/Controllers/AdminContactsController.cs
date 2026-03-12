using Microsoft.AspNetCore.Mvc;
using TechGearStore.Data;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var contacts = _context.ContactInfos.ToList();

            return View(contacts);
        }
    }
}