using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class WordQuestion
    {
        public Guid WordId { get; set; }
        [ForeignKey(nameof(WordId))]
        public virtual Word Word { get; set; }
        public Guid QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; }
    }
}