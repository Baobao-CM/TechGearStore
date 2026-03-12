using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechGearStore.Data;
using TechGearStore.Models;
using X.PagedList;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace TechGearStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bật khi đã có authentication
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // INDEX
        public IActionResult Index(string search, string role, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };

            var pagedUsers = users.OrderByDescending(u => u.CreatedAt)
                                  .ToPagedList(pageNumber, pageSize);

            return View(pagedUsers);
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // CREATE GET
        public IActionResult Create()
        {
            ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại.");
                    ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
                    return View(user);
                }

                if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
                {
                    ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự.");
                    ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
                    return View(user);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                user.CreatedAt = DateTime.UtcNow;
                _context.Add(user);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
            return View(user);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
            return View(user);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (existing.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại.");
                    ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
                    return View(user);
                }

                existing.FullName = user.FullName;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.IsActive = user.IsActive;

                _context.Update(existing);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                }
            }

            ViewBag.Roles = new[] { "Admin", "Staff", "Customer" };
            return View(user);
        }

        // CHANGE PASSWORD
        public async Task<IActionResult> ChangePassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View(await _context.Users.FindAsync(id));
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 8 ký tự.");
                return View(await _context.Users.FindAsync(id));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            _context.Update(user);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi đổi mật khẩu: " + ex.Message);
                return View(user);
            }
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Ngăn xóa admin cuối cùng
            if (user.Role == "Admin" && await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive) <= 1)
            {
                TempData["Error"] = "Không thể xóa admin cuối cùng để tránh mất quyền quản trị!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa người dùng {user.FullName} ({user.Email}) thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}