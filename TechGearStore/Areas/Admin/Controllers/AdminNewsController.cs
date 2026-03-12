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
    public class AdminNewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminNewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // INDEX
        // ==========================
        public IActionResult Index(string search, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var news = _context.News
                .OrderByDescending(n => n.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                news = news.Where(n => n.Title.Contains(search));
            }

            ViewBag.Search = search;

            var pagedNews = news.ToPagedList(pageNumber, pageSize);

            return View(pagedNews);
        }

        // ==========================
        // CREATE
        // ==========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news)
        {
            if (ModelState.IsValid)
            {
                if (news.ImageFile != null)
                {
                    news.ImageUrl = await UploadImage(news.ImageFile);
                }
                else
                {
                    news.ImageUrl = "/images/no-image.png";
                }

                news.CreatedDate = DateTime.Now;

                _context.News.Add(news);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(news);
        }

        // ==========================
        // EDIT
        // ==========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);

            if (news == null) return NotFound();

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news)
        {
            if (id != news.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var oldNews = await _context.News.FindAsync(id);

                if (oldNews == null)
                    return NotFound();

                oldNews.Title = news.Title;
                oldNews.Content = news.Content;

                if (news.ImageFile != null)
                {
                    // xoá ảnh cũ
                    if (!string.IsNullOrEmpty(oldNews.ImageUrl))
                    {
                        var oldPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            oldNews.ImageUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    oldNews.ImageUrl = await UploadImage(news.ImageFile);
                }

                _context.Update(oldNews);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(news);
        }

        // ==========================
        // DETAILS
        // ==========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null) return NotFound();

            return View(news);
        }
        // ==========================
        // DELETE
        // ==========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);

            if (news == null) return NotFound();

            return View(news);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);

            if (news != null)
            {
                // xoá ảnh
                if (!string.IsNullOrEmpty(news.ImageUrl))
                {
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        news.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // UPLOAD IMAGE
        // ==========================
        private async Task<string> UploadImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowExt = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowExt.Contains(extension))
                throw new Exception("File không hợp lệ");

            var fileName = Guid.NewGuid().ToString() + extension;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/news");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/news/" + fileName;
        }
    }
}