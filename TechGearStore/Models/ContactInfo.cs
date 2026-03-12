using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class ContactInfo
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Message { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}