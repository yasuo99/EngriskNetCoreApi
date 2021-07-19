using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class Category
    {
        public Guid WordId { get; set; }
        [ForeignKey(nameof(WordId))]
        public virtual Word Word { get; set; }
        public Guid WordCategoryId { get; set; }
        [ForeignKey(nameof(WordCategoryId))]
        public virtual WordCategory WordCategory{get;set;}
        public bool IsToeicVocabulary { get; set; }
    }
}