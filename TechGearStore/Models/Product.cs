using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        // Thumbnail
        public string? ImageUrl { get; set; }

        // Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Gallery images
        public ICollection<ProductImage>? Images { get; set; }
    }
}