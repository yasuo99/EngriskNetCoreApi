using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class QuizQuestion
    {
        public Guid QuizId { get; set; }
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
        public int Index { get; set; }
    }
}