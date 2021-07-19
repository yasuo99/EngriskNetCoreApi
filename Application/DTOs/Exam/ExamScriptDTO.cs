using System;
using Domain.Enums;

namespace Application.DTOs.Exam
{
    public class ExamScriptDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public DifficultLevel Difficult { get; set; }
    }
}