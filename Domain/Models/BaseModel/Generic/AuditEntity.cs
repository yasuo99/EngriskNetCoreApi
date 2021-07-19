using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.BaseModel.Generic
{
    public abstract class AuditEntity<TKey> : EntityBase<TKey>, IAuditEntity<TKey>
    {
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}