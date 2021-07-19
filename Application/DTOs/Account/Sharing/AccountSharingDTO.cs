using System;
using System.Collections.Generic;

namespace Application.DTOs.Account.Sharing
{
    public class AccountSharingDTO
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public string QuizTitle { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; }
        public List<string> Shared{ get; set; }
    }
}