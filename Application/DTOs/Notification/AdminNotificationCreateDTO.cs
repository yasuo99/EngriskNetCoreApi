using System.Collections.Generic;

namespace Application.DTOs.Notification
{
    public class AdminNotificationCreateDTO
    {
        public AdminNotificationCreateDTO()
        {
            Receivers = new List<int>();
        }
        public List<int> Receivers { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }
}