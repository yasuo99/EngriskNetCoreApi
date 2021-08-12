using System;
using System.Collections.Generic;
using Application.DTOs.Account;
using Application.DTOs.Question;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Quiz
{
    public class QuizDTO
    {
        public Guid Id { get; set; }
        public string QuizName { get; set; }
        public string DifficultLevel { get; set; }
        public string CreatedBy { get; set; }
        public string PublishStatus { get; set; }
        public int AccessCount { get; set; }
        public virtual AccountBlogDTO Owner { get; set; }
        public virtual List<QuestionDTO> Questions { get; set; }
    }
}