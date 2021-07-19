using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Post
{
    public class PostCreateDTO
    {
        public string Title { get; set; } 
        public int AccountId { get; set; }
        public string Content { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}