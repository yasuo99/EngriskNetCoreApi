
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
            Answers = new List<AnswerDTO>();
        }
        public Guid Id { get; set; }
        public string Audio { get; set; }
        public string PhotoUrl { get; set; }
        public string PreQuestion { get; set; }
        public string Content { get; set; }
        public ICollection<AnswerDTO> Answers { get; set; }
        public string Status { get; set; }
        public string Toeic { get; set; }
        public string Type { get; set; }
        public int Score { get; set; }
    }
}