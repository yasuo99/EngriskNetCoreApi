using System;
using System.Collections.Generic;
using System.Threading;

namespace Application.DTOs.ExamSchedule
{
    public class ExamScheduleDTO
    {
        public Guid ExamId { get; set; }
        public HashSet<Domain.Models.Question> Questions { get; set; }
        public Timer StartTimer { get; set; }
        public Timer QuestionTimer { get; set; }
        public int ElapseTime { get; set; }
    }
}