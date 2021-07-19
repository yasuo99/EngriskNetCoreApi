using Application.DTOs.Exam;
using Application.DTOs.Pagination;
using Application.DTOs.Quiz;

namespace Application.DTOs.Admin
{
    public class QuizAndExamCensorDTO
    {
        public PaginateDTO<QuizDTO> Quizzes { get; set; }
        public PaginateDTO<ExamDTO> Exams { get; set; }
    }
}