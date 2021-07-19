using System;
using System.Collections.Generic;
using Application.DTOs.Answer;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Question
{
    public class QuestionUpdateDTO
    {
        public Guid Id { get; set; }
        public IFormFile Image { get; set; }
        public bool IsAudioQuestion { get; set; }
        public string EngVoice { get; set; } = "en-US";
        public string Content { get; set; }
        public int Duration { get; set; }
        public ToeicPart ToeicPart { get; set; }
        public string Explaination { get; set; }
        public bool IsListeningQuestion { get; set; }
        public bool IsFillOutQuestion { get; set; }
        public bool IsArrangeQuestion { get; set; }
        public bool IsQuizQuestion { get; set; }
        public bool IsReadingQuestion { get; set; }
        public bool IsConnectionQuestion { get; set; }
        public int Score { get; set; }
        public List<AnswerUpdateDTO> Answers { get; set; }
    }
}