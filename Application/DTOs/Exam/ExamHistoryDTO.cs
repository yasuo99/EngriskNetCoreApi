using System;

namespace Application.DTOs.Exam
{
    public class ExamHistoryDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; }
        public DateTime Timestamp_start { get; set; }
        public DateTime Timestamp_end { get; set; }
        public int TotalTime { get; set; }
        public bool IsDone { get; set; }
        public bool IsPause { get; set; }
        public int Exp { get; set; }
        public int Score { get; set; }
    }
}