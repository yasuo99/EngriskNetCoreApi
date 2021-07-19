using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class Log: AuditEntity<Guid>
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}