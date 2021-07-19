using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Ticket.VerifyTicket
{
    public class UpdateVerifyTicketDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        [Required]
        public string Institue { get; set; }
        public string Email { get; set; }
        public IFormFile Image { get; set; }
        public DateTime? LicenseExpire { get; set; }
    }
}