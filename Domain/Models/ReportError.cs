using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class ReportError
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
        public string Error { get; set; }
        public DateTime ReportDate { get; set; }
    }
}