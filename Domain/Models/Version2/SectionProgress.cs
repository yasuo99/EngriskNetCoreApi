using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Domain.Models.Version2
{
    public class SectionProgress
    {
        public SectionProgress()
        {
            Details = new HashSet<SectionDetailProgress>();
        }
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; }
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
         [JsonIgnore]
        public virtual Account Account { get; set; }
        public bool IsDone { get; set; } = false;
        public bool IsLastDoing { get; set; } = false;
        public bool IsLock { get; set; } = true;
        [JsonIgnore]
        public virtual ICollection<SectionDetailProgress> Details { get; set; }
    }
}