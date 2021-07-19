using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class Script : AuditEntity<Guid>
    {
        public Script()
        {
            Questions = new HashSet<ScriptQuestion>();
            Words = new HashSet<ScriptWord>();
        }
        public string Theory { get; set; }
        public Guid SectionId { get; set; }
        public virtual Section Section { get; set; }
        public int Index { get; set; }
        public int VocabularySetting { get; set; } = 1;
        public ScriptTypes Type { get; set; }
        public virtual Exam MiniExam { get; set; }
        public virtual ICollection<ScriptQuestion> Questions { get; set; }
        public virtual ICollection<ScriptWord> Words { get; set; }
        public virtual ICollection<SectionDetailProgress> Progresses { get; set; }
        
    }
}