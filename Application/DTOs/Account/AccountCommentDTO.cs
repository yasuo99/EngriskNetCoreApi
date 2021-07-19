using System;

namespace Application.DTOs.Account
{
    public class AccountCommentDTO
    {
        public int PostId { get; set; }
        public int AccountId { get; set; }
        public string AccountUsername { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
    }
}