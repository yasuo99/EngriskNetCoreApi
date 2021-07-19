using System;
using System.Collections.Generic;
using Application.DTOs.Answer;

namespace Application.DTOs.Exam
{
    public class ClientExamDTO
    {
        public ClientExamDTO()
        {
            Answers = new HashSet<ClientAnswerDTO>();
        }
        public Guid ExamId { get; set; }
        public int Ranking { get; set; }
        public int Result { get; set; }
        public int Platform { get; set; }
        public HashSet<ClientAnswerDTO> Answers { get; set; }
    }
}