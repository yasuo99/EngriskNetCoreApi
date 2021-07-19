using System;
using Application.DTOs.Account;
using Application.DTOs.Comment;
using Application.DTOs.Pagination;
using Application.DTOs.Post;

namespace Application.DTOs.Admin
{
    public class CommentCensorDTO
    {
        public Guid Id { get; set; }
        public virtual AccountBlogDTO Account { get; set; }
        public virtual PostCensorDTO Post { get; set; }
        public string CreatedBy { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}