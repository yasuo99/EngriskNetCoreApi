using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class BoxChat: AuditEntity<Guid>
    {
        public BoxChat()
        {
            Members = new HashSet<BoxChatMember>();
            Messages = new HashSet<Message>();
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public bool IsPrivate { get; set; } = true;
        public string ChatKey{get;set;}
        public ICollection<BoxChatMember> Members { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}