using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class Message : AuditEntity<Guid>
    {
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public Guid BoxChatId { get; set; }
        [ForeignKey(nameof(BoxChatId))]
        public virtual BoxChat BoxChat { get; set; }
    }
}