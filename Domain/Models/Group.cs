using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models
{
    public class Group: AuditEntity<Guid>
    {
        public Group()
        {
            Words = new HashSet<WordGroup>();
        }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public string GroupName { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ICollection<WordGroup> Words { get; set; }
    }
}