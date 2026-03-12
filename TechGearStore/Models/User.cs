using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }  // Đổi tên từ Password → PasswordHash

        [Required]
        public string Role { get; set; }  // Ví dụ: "Admin", "Customer", "Staff"

        public bool IsActive { get; set; } = true;  // Mặc định active

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: thêm sau nếu cần
        // public string PhoneNumber { get; set; }
        // public string Address { get; set; }
    }
}