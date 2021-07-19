using System;

namespace Application.DTOs.Quiz
{
    public class QuizHistoryDTO
    {
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DoneDate { get; set; }
        public int TimeSpent { get; set; }
    }
}