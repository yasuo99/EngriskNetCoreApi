using System;
using System.Collections.Generic;
using Application.DTOs.Example;
using Application.DTOs.Question;
using Application.DTOs.Word.WordCategory;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Word
{
    public class WordUpdateDTO
    {
        public string Eng { get; set; }
        public string EngVoice { get; set; }
        public string Vie { get; set; }
        public IFormFile Image { get; set; }
        public string WordSpelling { get; set; }
        public WordClasses Class { get; set; }
        public List<WordCategoryDTO> Categories{get;set;}
        public IEnumerable<ExampleDTO> Examples { get; set; }
        public virtual IEnumerable<QuestionDTO> Questions { get; set; }
    }
}