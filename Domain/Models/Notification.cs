using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;

namespace Domain.Models
{
    public class Notification : AuditEntity<Guid>
    {
        public Notification()
        {
            To = new HashSet<AccountNotification>();
        }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public NotificationType Type { get; set; }
        public int? FromId { get; set; }
        [ForeignKey(nameof(FromId))]
        public virtual Account From { get; set; }
        public virtual ICollection<BoxChatMember> Invites { get; set; }
        public virtual ICollection<AccountNotification> To { get; set; }

    }
}