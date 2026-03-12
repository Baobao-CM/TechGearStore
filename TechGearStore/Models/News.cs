using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechGearStore.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        // Lưu đường dẫn ảnh
        public string ImageUrl { get; set; }

        // Thời gian tạo tin
        public DateTime CreatedDate { get; set; }

        // File upload (không lưu database)
        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}