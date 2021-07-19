using System;
using System.Collections.Generic;

namespace Application.DTOs.Exam
{
    public class ExamSharingDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string SharingTo { get; set; }
    }
}