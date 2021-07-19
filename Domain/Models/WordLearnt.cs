using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models
{
    public class WordLearnt: AuditEntity<Guid>
    {
        public WordLearnt()
        {
            Date = DateTime.Now.Date;
        }
        public Guid WordId { get; set; }
        [ForeignKey("WordId")]
        public virtual Word Word { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public DateTime Date { get; set; }
        public double AnswerTime{get;set;}
    }
}