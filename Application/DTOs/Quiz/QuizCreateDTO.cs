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
            TempQuestions = new List<QuestionCreateDTO>();
            QuestionImages = new List<IFormFile>();
            AnswerImages = new List<IFormFile>();
        }
        public Guid SectionId { get; set; }
        [Required]
        public string QuizName { get; set; }
        public string Detail { get; set; }
        public int Exp { get; set; }
        public bool IsPrivate { get; set; }
        public bool RequireLogin { get; set; }
        public DifficultLevel DifficultLevel { get; set; }
        public List<IFormFile> QuestionImages { get; set; }
        public List<IFormFile> AnswerImages { get; set; }
        public List<QuestionCreateDTO> TempQuestions{get;set;}
        public string SerializeQuestions { get; set; }
    }
}