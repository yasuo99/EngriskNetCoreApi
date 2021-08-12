using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class History
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid QuizId { get; set; }
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime EndTimestamp{get;set;}
        public bool IsDone { get; set; }
        public int TimeSpent { get; set; }
    }
}