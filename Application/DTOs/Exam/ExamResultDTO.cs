using System;
using Application.DTOs.Certificate;

namespace Application.DTOs.Exam
{
    public class ExamResultDTO
    {
        public int Score { get; set; }
        public int Listening { get; set; }
        public int Reading { get; set; }
        public bool IsCertificateExam { get; set; }
        public CertificateDTO Certificate { get; set; }
        public string Purpose { get; set; }
        public bool IsFinishScript { get; set; }
        public bool IsAbleToGetCertificate { get; set; }
        public DateTime Timestamp_start { get; set; }
        public DateTime Timestamp_end { get; set; }
        public int Spent { get; set; }
        public ExamAnswerDTO Answer { get; set; }
    }
}