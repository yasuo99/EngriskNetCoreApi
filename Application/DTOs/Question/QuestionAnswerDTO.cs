using System;
using System.Collections.Generic;
using Application.DTOs.Answer;

namespace Application.DTOs.Question
{
    public class QuestionAnswerDTO
    {
        public Guid Id { get; set; }
        public string ImageFileName { get; set; }
        public string AudioFileName {get;set;}
        public string Content { get; set; }
        public List<AnswerDTO> Answers { get; set; }
        public string Answer { get; set; }
        public string Explaination { get; set; }
        public string ToeicPart { get; set; }
        public string Type { get; set; }
        public string UserAnswer { get; set; }
        public bool IsRightAnswer { get; set; }
    }
}