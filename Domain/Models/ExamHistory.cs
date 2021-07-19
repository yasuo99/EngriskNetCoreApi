using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class ExamHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public DateTime Timestamp_start { get; set; }
        public DateTime Timestamp_end { get; set; }
        public bool IsDoing { get; set; }
        public bool IsDone { get; set; }
        public DateTime Timestamp_pause { get; set; }
        public int CurrentQuestion { get; set; }
        public int TotalTime { get; set; }
        public int Score { get; set; }
    }
}