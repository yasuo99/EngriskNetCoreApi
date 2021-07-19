using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Section
{
    public class SectionCreateDTO
    {
        public IFormFile File { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
        [Required]
        public string SectionName { get; set; }
        public string Description { get; set; }
        public int DPA { get; set; }
    }
}