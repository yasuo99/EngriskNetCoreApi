using System;
using System.Collections.Generic;
using Application.DTOs.Question;
using Domain.Enums;

namespace Application.DTOs.Exam
{
    public class ExamAnalyzeDTO
    {
        public ExamAnalyzeDTO()
        {
            
        }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Difficult { get; set; }
        public int Duration { get; set; }
        public DifficultLevel DifficultLevel { get; set; }
        public Dictionary<string,int> DonePie { get; set; }
        public Dictionary<string,int> GradePie { get; set; }
        public Dictionary<string,int> ScorePie { get; set; }
        public virtual List<QuestionAnalyzeDTO> Questions { get; set; }
    }
}