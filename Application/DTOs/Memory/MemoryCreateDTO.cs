using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Memory
{
    public class MemoryCreateDTO
    {
        public Guid WordId { get; set; }
        [MaxLength(50, ErrorMessage= "Tiêu đề dài tối đa 50 kí tự")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Hình của thẻ nhớ không được để trống")]
        public IFormFile Image { get; set; }
    }
}