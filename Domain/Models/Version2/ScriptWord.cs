using System;

namespace Domain.Models.Version2
{
    public class ScriptWord
    {
        public Guid ScriptId { get; set; }
        public virtual Script Script { get; set; }
        public Guid WordId { get; set; }
        public virtual Word Word { get; set; }

    }
}