using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models
{
    public class Badge: AuditEntity<Guid>
    {
        public string BadgeName { get; set; }
        public virtual ICollection<AccountBadge> Accounts { get; set; }
    }
}