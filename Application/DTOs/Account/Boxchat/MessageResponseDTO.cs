using System;

namespace Application.DTOs.Account.Boxchat
{
    public class MessageResponseDTO
    {
        public Guid BoxchatId { get; set; }
        public string Text { get; set; }
        public Guid Id { get; set; }
        public Sender Sender { get; set; }
    }
    public class Sender{
        public string Name { get; set; }
        public string Uid { get; set; }
        public string Avatar { get; set; }
    }
}