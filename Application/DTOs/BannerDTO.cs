using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public class BannerDTO
    {
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
        public IFormFile File { get; set; }
    }
}