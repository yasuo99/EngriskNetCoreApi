using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;

namespace Domain.Models
{
    //Model post: bài viết của người dùng đăng trên diễn đàn, có upvote và downvote để sử dụng thuật toán làm mới bài viết
    public class Post : AuditEntity<Guid>
    {
        public Post()
        {
            PostImages = new HashSet<PostImage>();
            Comments = new HashSet<Comment>();
            Documents = new HashSet<Document>();
            LikedPosts = new HashSet<LikedPost>();
            VerifiedStatus = Status.Pending;
        }
        public int? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPrivate { get; set; }
        public Status VerifiedStatus { get; set; }
        public int AccessCount { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<LikedPost> LikedPosts { get; set; }
        public virtual ICollection<PostImage> PostImages { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
    }
}