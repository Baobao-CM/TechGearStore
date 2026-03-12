using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TechGearStore.Data;
using TechGearStore.Models;
using X.PagedList;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminEventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminEventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // DANH SÁCH SỰ KIỆN
        // =========================
        public IActionResult Index(string search, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var events = _context.Events
                .OrderByDescending(e => e.EventDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                events = events.Where(e => e.Title.Contains(search));
            }

            ViewBag.Search = search;

            return View(events.ToPagedList(pageNumber, pageSize));
        }

        // =========================
        // CHI TIẾT
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // =========================
        // TẠO SỰ KIỆN
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev)
        {
            if (ModelState.IsValid)
            {
                ev.CreatedDate = DateTime.Now;

                if (ev.ImageFile != null)
                {
                    ev.ImageUrl = await UploadImage(ev.ImageFile);
                }

                _context.Add(ev);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ev);
        }

        // =========================
        // SỬA SỰ KIỆN
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev)
        {
            if (id != ev.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var oldEvent = await _context.Events.FindAsync(id);

                if (oldEvent == null)
                    return NotFound();

                oldEvent.Title = ev.Title;
                oldEvent.Description = ev.Description;
                oldEvent.EventDate = ev.EventDate;

                if (ev.ImageFile != null)
                {
                    // xoá ảnh cũ
                    if (!string.IsNullOrEmpty(oldEvent.ImageUrl))
                    {
                        var oldPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            oldEvent.ImageUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    oldEvent.ImageUrl = await UploadImage(ev.ImageFile);
                }

                _context.Update(oldEvent);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ev);
        }

        // =========================
        // XÓA
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev != null)
            {
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                {
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        ev.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // UPLOAD IMAGE
        // =========================
        private async Task<string> UploadImage(IFormFile file)
        {
            if (file == null)
                return null;

            var extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowExt = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowExt.Contains(extension))
                throw new Exception("File không hợp lệ");

            var fileName = Guid.NewGuid().ToString() + extension;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/events");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/events/" + fileName;
        }
    }
}