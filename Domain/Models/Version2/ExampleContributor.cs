using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain.Models.Version2
{
    public class ExampleContributor
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public Guid ExampleId { get; set; }
        public virtual Example Example { get; set; }
        public DateTime Timestamp { get; set; }
    }
}