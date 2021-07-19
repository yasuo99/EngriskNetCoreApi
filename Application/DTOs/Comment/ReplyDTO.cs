using System;

namespace Application.DTOs.Comment
{
    public class ReplyDTO
    {
        public Guid Id { get; set; }
        public Guid ReplyId { get; set; }
        public int AccountId { get; set; }
        public string AccountPhotoUrl { get; set; }
        public string AccountUsername { get; set; }
        public bool AccountVerified { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        public bool IsEdited { get; set; }
    }
}