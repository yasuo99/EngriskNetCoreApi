using System.Collections.Generic;
using Application.DTOs.Hub;
using Application.DTOs.Notification;

namespace Application.Helper
{
    public static class HubHelper
    {
        public static HashSet<ExamClient> ExamClientsConnections = new HashSet<ExamClient>();
        public static HashSet< NotificationClient> NotificationClientsConnections = new HashSet<NotificationClient>();
    }
}