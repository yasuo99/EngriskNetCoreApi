using System.Collections.Generic;
using Application.DTOs.Question;
using Application.DTOs.Word;

namespace Application.DTOs.Script
{
    public class ScriptDTO
    {
        public ScriptDTO()
        {
            Words = new List<WordDTO>();
            Questions = new List<QuestionDTO>();
        }
        public List<WordDTO> Words { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}