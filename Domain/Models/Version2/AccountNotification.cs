using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models;

namespace Domain.Models.Version2
{
    public class AccountNotification
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid NotificationId { get; set; }
        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; }
        public NotificationStatus Status { get; set; }
    }
}