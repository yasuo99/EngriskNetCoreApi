using System.Collections.Generic;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Word.WordCategory
{
    public class WordCategoryCreateDTO
    {
        public string CategoryName { get; set; }
        public IFormFile Image { get; set; }
        public List<WordCategoryTag> Tags { get; set; }
    }
}