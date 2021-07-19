using System;
using System.Collections.Generic;
using Application.DTOs.Account;
using Application.DTOs.Exam;
using Application.DTOs.Question;
using Domain.Models;

namespace Application.DTOs.Exam
{
    public class ExamDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string CreatedBy { get; set; }
        public virtual AccountBlogDTO Owner { get; set; }
        public string Difficult { get; set; }
        public int ExpGain { get; set; }
        public int TotalListening { get; set; }
        public int TotalReading { get; set; }
        public int TotalScore { get; set; }
        public int Duration { get; set; }
        public bool IsNew { get; set; }
        public int TimeRemain { get; set; }
        public bool IsPrivate { get; set; }
        public virtual IEnumerable<QuestionDTO> Questions { get; set; }
        public virtual IEnumerable<ExamHistoryDTO> ExamHistories { get; set; }
    }
}