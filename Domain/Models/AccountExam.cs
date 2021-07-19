using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountExam
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }
        public DateTime Created_At { get; set; }
        public string SharedUrl { get; set; }
        public bool IsPublished { get; set; }
        public int AccessCount { get; set; }
    }
}