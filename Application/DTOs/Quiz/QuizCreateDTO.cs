using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTOs.Question;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Quiz
{
    public class QuizCreateDTO
    {
        public QuizCreateDTO()
        {
        }
        [Required]
        public string QuizName { get; set; }
        public string Detail { get; set; }
        public bool IsPrivate { get; set; }
        public bool RequireLogin { get; set; }
    }
}