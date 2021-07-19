
using System;
using System.Collections.Generic;
using Application.DTOs.Answer;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Application.DTOs.Question
{
    public class QuestionDTO
    {
        public QuestionDTO()
        {
            Answers = new HashSet<AnswerDTO>();
        }
        public Guid Id { get; set; }
        public string Audio { get; set; }
        public string PhotoUrl { get; set; }
        public string PreQuestion { get; set; }
        public string Content { get; set; }
        public HashSet<AnswerDTO> Answers { get; set; }
        public string Toeic { get; set; }
        public string Type { get; set; }
        public int Score { get; set; }
    }
}