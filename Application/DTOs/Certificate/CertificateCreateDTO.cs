using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Certificate
{
    public class CertificateCreateDTO
    {
        public string Subject { get; set; }
        public string Title { get; set; }
        public int LifeTime { get; set; }
        public IFormFile Template { get; set; }
    }
}