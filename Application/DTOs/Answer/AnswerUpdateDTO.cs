using System;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Answer
{
    public class AnswerUpdateDTO
    {
        public Guid Id { get; set; }
        public string Content{get;set;}
        public IFormFile Image { get; set; }
        public bool IsAudioAnswer { get; set; }
        public bool IsQuestionAnswer { get; set; }
    }
}