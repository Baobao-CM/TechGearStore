using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Quan hệ 1 Category -> nhiều Product
        public ICollection<Product>? Products { get; set; }
    }
}