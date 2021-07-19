using System;
using System.Collections.Generic;
using Application.DTOs.Question;

namespace Application.DTOs.Word
{
    public class WordQuestionsCreateDTO
    {
        public Guid WordId { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}