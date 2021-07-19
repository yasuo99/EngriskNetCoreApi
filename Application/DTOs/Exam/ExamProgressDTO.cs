using System;

namespace Application.DTOs.Exam
{
    public class ExamProgressDTO
    {
        public Guid ExamId { get; set; }
        public int AccountId { get; set; }
        public int Score { get; set; }
        public DateTime End_At { get; set; }
        public int TotalTime { get; set; }
    }
}