using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}