using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class WordCategory: AuditEntity<Guid>
    {
        public WordCategory()
        {
            Tags = new HashSet<WordCategoryTag>();
        }
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string Description { get; set; }
        public bool IsToeicVocabulary { get; set; }
        public Guid? SectionId { get; set; }
        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; }
        public virtual ICollection<Category> Words { get; set; }
        public virtual ICollection<WordCategoryTag> Tags { get; set; }
    }
}