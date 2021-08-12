using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Version2;

namespace Domain.Models
{
    public class ExamHistory
    {
        public ExamHistory()
        {
            AccountAnswers = new HashSet<AccountAnswer>();
        }
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
        public int TotalTime { get; set; }
        public bool ReceivedCertificate { get; set; }
        public int Score { get; set; }
        public int Listening { get; set; }
        public int Reading { get; set; }
        public virtual ICollection<AccountAnswer> AccountAnswers { get; set; }
    }
}