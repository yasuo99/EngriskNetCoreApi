using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Exam
{
    public class SelfExamDTO
    {
        public string ExamTitle { get; set; }
        public IFormFile File { get; set; }
        public int TimeDuration { get; set; }
        public string Description { get; set; }
        public bool isShared { get; set; }
    }
}