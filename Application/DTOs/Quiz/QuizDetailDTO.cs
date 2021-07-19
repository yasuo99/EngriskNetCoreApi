using System;
using System.Collections.Generic;

namespace Application.DTOs.Quiz
{
    public class QuizDetailDTO
    {
        public Guid Id { get; set; }
        public string QuizPhoto { get; set; }
        public string QuizName { get; set; }
        public int DifficultLevel { get; set; }
        public int ExpGain { get; set; }
        public int Enrolled { get; set; }
        public int DurationTime { get; set; }
        public bool RequireLogin { get; set; }
        public int TotalQuestion { get; set; }
    }
}