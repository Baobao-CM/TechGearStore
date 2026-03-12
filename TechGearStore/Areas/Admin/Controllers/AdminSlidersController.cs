using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechGearStore.Data;
using TechGearStore.Models;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminSlidersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSlidersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminSliders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        // GET: Admin/AdminSliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Admin/AdminSliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminSliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Link,ImageFile")] Slider slider)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (slider.ImageFile != null)
                    {
                        slider.ImageUrl = await UploadImage(slider.ImageFile);
                    }
                    _context.Add(slider);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm slider thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi upload ảnh: " + ex.Message);
                }
            }
            return View(slider);
        }

        // GET: Admin/AdminSliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Admin/AdminSliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Link,ImageFile")] Slider slider)
        {
            if (id != slider.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oldSlider = await _context.Sliders.FindAsync(id);
                    if (oldSlider == null) return NotFound();

                    oldSlider.Title = slider.Title;
                    oldSlider.Link = slider.Link;

                    if (slider.ImageFile != null)
                    {
                        // Optional: xóa ảnh cũ nếu muốn
                        // if (!string.IsNullOrEmpty(oldSlider.ImageUrl)) { ... xóa file }

                        oldSlider.ImageUrl = await UploadImage(slider.ImageFile);
                    }

                    _context.Update(oldSlider);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật slider thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật.");
                }
            }
            return View(slider);
        }

        // GET: Admin/AdminSliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Admin/AdminSliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                _context.Sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
        private async Task<string> UploadImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowExt = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowExt.Contains(extension))
                throw new Exception("File không hợp lệ");

            var fileName = Guid.NewGuid().ToString() + extension;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/sliders");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/sliders/" + fileName;
        }
    }
}
