using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class WordCategoryTag
    {
        public Guid WordCategoryId { get; set; }
        [ForeignKey(nameof(WordCategoryId))]
        public virtual WordCategory WordCategory { get; set; }
        public Guid CategoryTagId { get; set; }
        [ForeignKey(nameof(CategoryTagId))]
        public virtual CategoryTag CategoryTag { get; set; }
    }
}