namespace Application.DTOs.Exam
{
    public class ExamResultDTO
    {
        public int Score { get; set; }
        public int Listening { get; set; }
        public int Reading { get; set; }
        public ExamAnswerDTO Answer { get; set; }
    }
}