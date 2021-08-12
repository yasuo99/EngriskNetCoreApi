using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTOs.Example;
using Application.DTOs.Word.WordCategory;
using Domain.Enums;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Word
{
    public class WordCreateDTO
    {
        public WordCreateDTO()
        {
            Categories = new List<WordCategoryDTO>();
            Examples = new List<ExampleDTO>();
        }
        public IFormFile Image { get; set; }
        [Required]
        public string Eng { get; set; }
        public string Spelling { get; set; }
        public string EngVoice { get; set; } = "en-US";
        public string Vie { get; set; }
        public WordClasses Class{get;set;}
        public List<WordCategoryDTO> Categories{get;set;}
        public List<ExampleDTO> Examples { get; set; }
    }
}