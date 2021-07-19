using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;
using Newtonsoft.Json;

namespace Application.DTOs.Post
{
    public class PostDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string AccountPhotoUrl { get; set; }
        public string AccountUserName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalComment { get; set; }
        public bool IsLocked { get; set; }
    }
}