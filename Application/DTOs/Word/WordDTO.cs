using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Application.DTOs.Example;
using Application.DTOs.Question;
using Application.DTOs.Word.WordCategory;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Word
{
    public class WordDTO
    {
        public Guid Id { get; set; }
        public string WordImg { get; set; }
        public string Eng { get; set; }
        public string Spelling { get; set; }
        public string Vie { get; set; }
        public string WordVoice { get; set; }
        public string Class { get; set; }
        public bool IsLearned { get; set; } = false;
        public string PublishStatus { get; set;}
        public string FlashcardTitle
        {
            get
            {
                return IsLearned ? "Ghi nhớ từ" : "Đọc và nghe từ vựng mới";
            }
        }
        public QuestionDTO QuestionDTO { get; set; }
        public Domain.Models.Version2.Memory Memory { get; set; }
        public virtual IEnumerable<ExampleDTO> Examples { get; set; }
        public virtual IEnumerable<WordDTO> Synonyms { get; set; }
        public virtual IEnumerable<Domain.Models.Version2.Memory> Memories { get; set; }
        public virtual IEnumerable<WordCategoryDTO> Categories { get; set; }
        public virtual IEnumerable<WordDTO> Families { get; set; }
        public virtual IEnumerable<QuestionDTO> Questions { get; set; }
    }
}