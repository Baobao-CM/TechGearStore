using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechGearStore.Data;
using TechGearStore.Models;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // INDEX
        // ===============================
        public IActionResult Index(string search, int? categoryId, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var products = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.ProductId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "Name");

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // ===============================
        // DETAILS
        // ===============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // ===============================
        // CREATE
        // ===============================
        public IActionResult Create()
        {
            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            IFormFile imageFile,
            IFormFile[] galleryFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        product.ImageUrl = await UploadImage(imageFile);
                    }

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    if (galleryFiles != null)
                    {
                        foreach (var file in galleryFiles)
                        {
                            if (file != null)
                            {
                                var url = await UploadImage(file);

                                ProductImage img = new ProductImage
                                {
                                    ProductId = product.ProductId,
                                    ImageUrl = url
                                };

                                _context.ProductImages.Add(img);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Lỗi upload ảnh.");
                }
            }

            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // ===============================
        // EDIT
        // ===============================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    Product product,
    IFormFile imageFile,
    IFormFile[] galleryFiles)
        {
            if (id != product.ProductId)
                return NotFound();

            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
                return NotFound();

            ModelState.Remove("Images");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Thumbnail
                if (imageFile != null && imageFile.Length > 0)
                {
                    existingProduct.ImageUrl = await UploadImage(imageFile);
                }
                else
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                }

                // Gallery
                if (galleryFiles != null && galleryFiles.Length > 0)
                {
                    foreach (var file in galleryFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var url = await UploadImage(file);

                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = existingProduct.ProductId,
                                ImageUrl = url
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(existingProduct);
        }

        // ===============================
        // DELETE
        // ===============================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                foreach (var img in product.Images)
                {
                    _context.ProductImages.Remove(img);
                }

                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETE GALLERY IMAGE
        // ===============================
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);

            if (image == null)
                return NotFound();

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ===============================
        // HELPER UPLOAD IMAGE
        // ===============================
        private async Task<string> UploadImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowExt = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowExt.Contains(extension))
                throw new Exception("File không hợp lệ");

            var fileName = Guid.NewGuid().ToString() + extension;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/products");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/products/" + fileName;
        }
    }
}