using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountBadge
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid BadgeId { get; set; }
        [ForeignKey("BadgeId")]
        public virtual Badge Badge { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsMain { get; set; }
    }
}