using System;
using System.Collections.Generic;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class CategoryTag: AuditEntity<Guid>
    {
        public string Tag { get; set; }
        public virtual ICollection<WordCategoryTag> Categories { get; set; }
    }
}