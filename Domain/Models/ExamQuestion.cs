using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class ExamQuestion
    {
        public Guid ExamId { get; set; }
        [ForeignKey("ExamId ")]
        public virtual Exam Exam { get; set; }
        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
        public int Index { get; set; }
    }
}