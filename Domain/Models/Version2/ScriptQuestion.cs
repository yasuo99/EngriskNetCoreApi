using System;

namespace Domain.Models.Version2
{
    public class ScriptQuestion
    {
        public Guid ScriptId { get; set; }
        public virtual Script Script { get; set; }
        public Guid QuestionId { get; set; }
        public virtual Question Question { get; set; }
        public int Index { get; set; }
    }
}