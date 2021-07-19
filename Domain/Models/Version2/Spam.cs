using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models.Version2
{
    public class Spam
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Status Status { get; set; }
    }
}