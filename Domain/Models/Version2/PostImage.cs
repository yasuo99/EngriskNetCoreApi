using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class PostImage
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }
        public string ImageUrl { get; set; }
        public string FileName { get; set; }
    }
}