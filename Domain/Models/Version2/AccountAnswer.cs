using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class AccountAnswer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? ExamHistoryId { get; set; }
        public virtual ExamHistory ExamHistory { get; set; }
        public Guid QuestionId  { get; set; }
        public virtual Question Question { get; set; }
        public Guid AnswerId { get; set; }
        public virtual Answer Answer { get; set; }
        public bool Result { get; set; }
    }
}