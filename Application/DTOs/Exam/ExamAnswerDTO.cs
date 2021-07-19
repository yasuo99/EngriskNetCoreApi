using System;
using System.Collections.Generic;
using Application.DTOs.Question;

namespace Application.DTOs.Exam
{
    public class ExamAnswerDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int ExpGain { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
        public virtual IEnumerable<QuestionAnswerDTO> Questions { get; set; }
    }
}