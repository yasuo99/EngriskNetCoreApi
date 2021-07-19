using System;
using System.Collections.Generic;
using Application.DTOs.Account;
using Application.DTOs.Pagination;
using Application.DTOs.Post;
using Domain.Models.Version2;

namespace Application.DTOs.Admin
{
    public class PostCensorDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; }
        public virtual AccountBlogDTO Account { get; set; }
        public virtual ICollection<PostImage> PostImages { get; set; }
    }
}