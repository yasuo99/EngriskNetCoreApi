namespace Application.DTOs.Notification
{
    public class NotificationCreateDTO
    {
        public string Content { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool IsClientNotify { get; set; }
    }
}