using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class LikedComment
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid CommentId { get; set; }
        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }
        public DateTime Timestamp { get; set; }
    }
}