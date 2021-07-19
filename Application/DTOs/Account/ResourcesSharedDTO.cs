using System.Collections.Generic;
using Application.DTOs.Exam;
using Application.DTOs.Quiz;

namespace Application.DTOs.Account
{
    public class ResourcesSharedDTO
    {
        public List<ExamDTO> Exams { get; set; }
        public List<QuizDTO> Quizzes { get; set; }
    }
}