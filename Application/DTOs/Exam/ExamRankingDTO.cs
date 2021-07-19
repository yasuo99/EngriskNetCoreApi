using System;

namespace Application.DTOs.Exam
{
    public class ExamRankingDTO
    {
        public int AccountId { get; set; }
        public string AccountUsername { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int Score { get; set; }
        public int TotalScore { get; set; }
        public int TotalTime { get; set; }
    }
}