using System;

namespace Application.DTOs
{
    public class RatingDTO
    {
        public Guid Id { get; set; }
        public int AccountId  { get; set; }
        public string AccountUsername { get; set; }
        public int Rating { get; set; }
    }
}