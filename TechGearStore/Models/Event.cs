using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechGearStore.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime EventDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}