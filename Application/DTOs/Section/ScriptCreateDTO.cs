using System;
using System.Collections.Generic;
using Application.DTOs.Exam;
using Application.DTOs.Question;
using Application.DTOs.Word;
using Domain.Enums;

namespace Application.DTOs.Section
{
    public class ScriptCreateDTO
    {
        public ScriptCreateDTO()
        {
            Questions = new List<QuestionDTO>();
            Words = new List<WordDTO>();
        }
        public Guid Id { get; set; }
        public List<QuestionDTO> Questions { get; set; }
        public List<WordDTO> Words { get; set; }
        public string Theory { get; set; }
        public Guid Exam { get; set; }
        public int VocabularySetting { get; set; }
        public ScriptTypes Type { get; set; }
    }
}