using System;

namespace Application.DTOs.Answer
{
    public class AnswerAnalyzeDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public bool IsQuestionAnswer { get; set; }
        public int SelectCount { get; set; }
    }
}