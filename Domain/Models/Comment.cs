using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models
{
    public class Comment : EntityBase<Guid>
    {
        public Comment()
        {
            VerifiedStatus = Status.Pending;
            Timestamp = DateTime.Now;
        }
        public Guid? ReplyId { get; set; }
        [ForeignKey("ReplyId")]
        public virtual Comment Reply { get; set; }
        public Guid PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Status VerifiedStatus { get; set; }
        public virtual ICollection<Comment> Replies { get; set; }
        public virtual ICollection<LikedComment> LikedComments { get; set; }
    }
}