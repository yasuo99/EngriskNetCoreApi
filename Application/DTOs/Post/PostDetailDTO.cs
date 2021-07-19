using System;
using System.Collections.Generic;
using Application.DTOs.Comment;
using Domain.Models.Version2;

namespace Application.DTOs.Post
{
    public class PostDetailDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string AccountPhotoUrl { get; set; }
        public string AccountUsername { get; set; }
        public bool AccountVerified { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsLocked { get; set; }
        public virtual IEnumerable<CommentDTO> Comments { get; set; }
        public virtual ICollection<PostImage> PostImages { get; set; }
    }
}