using System;

namespace Application.DTOs.Answer
{
    public class ClientAnswerDTO
    {
        public Guid ExamId { get; set; }
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
        public int TimeSpent { get; set; }
        public bool Result { get; set; }
    }
}