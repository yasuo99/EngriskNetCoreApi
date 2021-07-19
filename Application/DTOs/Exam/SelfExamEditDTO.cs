using System.Collections.Generic;
using Application.DTOs.Question;
using Domain.Enums;

namespace Application.DTOs.Exam
{
    public class SelfExamEditDTO
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public int Duration { get; set; }
        public bool IsPrivate { get; set; }
        public int Exp { get; set; }
        public string PassCode { get; set; }
        public DifficultLevel Difficult { get; set; }
        public List<QuestionUpdateDTO> Questions { get; set; }

    }
}