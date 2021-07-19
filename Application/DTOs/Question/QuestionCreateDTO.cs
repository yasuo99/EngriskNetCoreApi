using System.Collections.Generic;
using Application.DTOs.Answer;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Question
{
    public class QuestionCreateDTO
    {
        public IFormFile Image { get; set; }
        public IFormFile Audio { get; set; }
        public bool IsAudioQuestion { get; set; }
        public string EngVoice { get; set; } = "en-US";
        public string PreQuestion { get; set; }
        public string Content { get; set; }
        public ToeicPart Toeic { get; set; }
        public string Explaination { get; set; }
        public QuestionType Type { get; set; }
        public int Score { get; set; }
        public int Duration { get; set; }
        public List<CreateAnswerDTO> Answers { get; set; }
    }
}