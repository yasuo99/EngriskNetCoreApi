using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;
using Newtonsoft.Json;

namespace Domain.Models
{
    public class Example : AuditEntity<Guid>
    {
        public Example()
        {
            VerifiedStatus = Status.Pending;
        }
        public Guid WordId { get; set; }
        [ForeignKey(nameof(WordId))]
        public virtual Word Word { get; set; }
        public string Eng { get; set; }
        public string Vie { get; set; }
        public Status VerifiedStatus { get; set; }
    }
}