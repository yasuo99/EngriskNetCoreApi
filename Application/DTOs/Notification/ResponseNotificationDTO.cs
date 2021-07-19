using System;
using Application.DTOs.Account;
using Domain.Enums;

namespace Application.DTOs.Notification
{
    public class ResponseNotificationDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public AccountBlogDTO From { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
    }
}