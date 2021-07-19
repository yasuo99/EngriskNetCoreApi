using System;
using Domain.Enums;

namespace Application.DTOs.Notification
{
    public class AccountNotificationDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public NotificationType Type { get; set; }
        public string CreatedBy { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}