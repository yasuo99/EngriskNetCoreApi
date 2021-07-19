using System;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Answer
{
    public class CreateAnswerDTO
    {
        public string Content{get;set;}
        public bool IsAudioAnswer { get; set; }
        public bool IsQuestionAnswer { get; set; }
    }
}