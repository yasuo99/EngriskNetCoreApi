using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain.Models.Version2
{
    public class WordContributor
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid WordId { get; set; }
        [ForeignKey("WordId")]
        public virtual Word Word { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Status { get; set; }
        public int? ApproverId { get; set; }
        [ForeignKey("ApproverId")]
        public virtual Account Approver { get; set; }
    }
}