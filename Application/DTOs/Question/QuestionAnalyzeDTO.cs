using System;
using System.Collections.Generic;
using Application.DTOs.Answer;
using Domain.Enums;

namespace Application.DTOs.Question
{
    public class QuestionAnalyzeDTO
    {
        public Guid Id { get; set; }
        public string Audio { get; set; }
         public string PhotoUrl { get; set; }
        public string PreQuestion { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public virtual List<AnswerAnalyzeDTO> Answers { get; set; }
    }
}