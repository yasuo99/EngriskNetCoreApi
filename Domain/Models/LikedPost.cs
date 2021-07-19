using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class LikedPost
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
        public DateTime Like_At { get; set; }
    }
}