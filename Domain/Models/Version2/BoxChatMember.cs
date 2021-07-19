using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models.Version2
{
    public class BoxChatMember
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public Guid BoxChatId { get; set; }
        [ForeignKey(nameof(BoxChatId))]
        public virtual BoxChat BoxChat { get; set; }
        public Guid? NotificationId { get; set; }
        public virtual Notification Notification { get; set; }
        public Status Status { get; set; }
    }
}