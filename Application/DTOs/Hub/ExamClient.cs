using Application.DTOs.Exam;

namespace Application.DTOs.Hub
{
    public class ExamClient
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string ClientId { get; set; }
        public int Platform { get; set; }
        public ClientExamDTO Exam { get; set; }
    }
}