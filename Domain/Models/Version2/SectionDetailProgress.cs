using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class SectionDetailProgress
    {
        public Guid SectionProgressId { get; set; }
        [ForeignKey(nameof(SectionProgressId))]
        public virtual SectionProgress SectionProgress { get; set; }
        public Guid ScriptId { get; set; }
        [ForeignKey(nameof(ScriptId))]
        public virtual Script Script { get; set; }
        public bool IsDone { get; set; }
    }
}