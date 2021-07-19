using System;

namespace Application.DTOs.Hub
{
    public class ReceiveMessageDTO
    {
        public Guid Id { get; set; }
        public int FromId { get; set; }
        public string FromAvatar { get; set; }
        public string FromUsername { get; set; }
        public string Content { get; set; }
    }
}