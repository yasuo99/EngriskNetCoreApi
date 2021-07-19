using System;

namespace Application.DTOs.Badge
{
    public class UpdateBadgeDTO
    {
        public Guid Id { get; set; }
        public string BadgeName { get; set; }
    }
}