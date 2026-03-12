using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechGearStore.Models
{
    public class Slider
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? ImageUrl { get; set; }

        public string? Link { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}