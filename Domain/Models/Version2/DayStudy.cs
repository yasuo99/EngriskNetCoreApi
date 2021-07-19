using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class DayStudy: AuditEntity<Guid>
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public int TotalSections { get; set; }
        public int TotalWords { get; set; } = 0;
        public int TotalExams { get; set; } = 0;
        public int TotalQuizzes { get; set; } = 0;
        public int TotalConversation { get; set; } = 0;
        public int TotalListening { get; set; } = 0;
        public int TotalGrammar { get; set; } = 0;
        public int TotalReading { get; set; } = 0;
        public int TotalWriting { get; set; } = 0;
        public int TotalVocabulary { get; set; } = 0;
    }
}