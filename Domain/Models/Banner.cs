using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}