using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedDate { get; set; }

        public Product Product { get; set; }

        public User User { get; set; }
    }
}