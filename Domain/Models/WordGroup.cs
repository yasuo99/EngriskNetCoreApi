using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class WordGroup
    {
        public Guid GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public Guid WordId { get; set; }
        [ForeignKey("WordId")]
        public virtual Word Word { get; set; }
    }
}