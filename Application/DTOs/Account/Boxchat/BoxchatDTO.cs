using System;
using System.Collections.Generic;
using Application.DTOs.Hub;

namespace Application.DTOs.Account.Boxchat
{
    public class BoxchatDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int AccountId { get; set; }
        public string PhotoUrl { get; set; }
        public string Description { get; set; }
        public List<BoxchatMemberDTO> Members { get; set; }
        public List<MessageResponseDTO> Messages { get; set; }
    }
}