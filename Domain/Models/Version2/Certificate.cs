using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class Certificate : AuditEntity<Guid>
    {
        public Certificate()
        {
            Accounts = new HashSet<AccountCertificate>();
        }
        public string Subject { get; set; }
        public string Title { get; set; }
        public string Template { get; set; }
        public int LifeTime { get; set; }
        public Guid? ScriptId { get; set; }
        [ForeignKey(nameof(ScriptId))]
        public virtual Script Script { get; set; }
        public virtual ICollection<AccountCertificate> Accounts { get; set; }

    }
}