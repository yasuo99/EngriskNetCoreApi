using System;

namespace Application.DTOs.Answer
{
    public class AnswerDTO
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
        public string ImageFileName { get; set; }
        public string AudioFileName { get; set; }
        public bool IsQuestionAnswer { get; set; }
    }
}