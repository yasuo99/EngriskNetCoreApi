using System;

namespace Application.DTOs.Hub
{
    public class MessageDTO
    {
        public Guid BoxchatId { get; set; }
        public int FromId { get; set; }
        public string FromUsername { get; set; }
        public string Content { get; set; }
    }
}