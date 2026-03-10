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

        // GET: Admin/AdminProducts
        public async Task<IActionResult> Index(string search, int? categoryId, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/AdminProducts/Details/5
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

        // GET: Admin/AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile, IFormFile[] galleryFiles)
        {
            if (ModelState.IsValid)
            {
                // Upload ảnh chính
                if (imageFile != null && imageFile.Length > 0)
                {
                    var extension = Path.GetExtension(imageFile.FileName);
                    var fileName = Guid.NewGuid().ToString() + extension;

                    var uploadPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images/products"
                    );

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var fullPath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/products/" + fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // ===== Upload nhiều ảnh gallery =====

                if (galleryFiles != null && galleryFiles.Length > 0)
                {
                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            var extension = Path.GetExtension(file.FileName);
                            var fileName = Guid.NewGuid().ToString() + extension;

                            var uploadPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot/images/products"
                            );

                            var fullPath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            ProductImage img = new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = "/images/products/" + fileName
                            };

                            _context.ProductImages.Add(img);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // POST: Admin/AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    Product product,
    IFormFile? imageFile,
    IFormFile[] galleryFiles)
        {
            if (id != product.ProductId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (existingProduct == null)
                    return NotFound();

                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // ===== Upload ảnh chính =====
                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            existingProduct.ImageUrl.TrimStart('/')
                        );

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var extension = Path.GetExtension(imageFile.FileName);
                    var fileName = Guid.NewGuid().ToString() + extension;

                    var uploadPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images/products"
                    );

                    var fullPath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    existingProduct.ImageUrl = "/images/products/" + fileName;
                }

                // ===== Upload gallery mới =====
                if (galleryFiles != null && galleryFiles.Length > 0)
                {
                    foreach (var file in galleryFiles)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var fileName = Guid.NewGuid().ToString() + extension;

                        var uploadPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/images/products"
                        );

                        var fullPath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        ProductImage img = new ProductImage
                        {
                            ProductId = existingProduct.ProductId,
                            ImageUrl = "/images/products/" + fileName
                        };

                        _context.ProductImages.Add(img);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // GET: Admin/AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // xóa ảnh khỏi server
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        product.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);

            if (image == null)
                return NotFound();

            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    image.ImageUrl.TrimStart('/')
                );

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            int productId = image.ProductId;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = productId });
        }
    }

}