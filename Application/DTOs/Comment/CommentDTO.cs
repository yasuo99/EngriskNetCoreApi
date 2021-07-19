using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public int AccountId { get; set; }
        public Guid ReplyId { get; set; }
        public string AccountPhotoUrl { get; set; }
        public string AccountUsername { get; set; }
        public bool IsVerified { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        public bool IsReplyComment { get; set; }
        public string VerifiedStatus { get; set; }
        public virtual IEnumerable<CommentDTO> Replies { get; set; }
    }
}