using System;
using System.Collections.Generic;

namespace Application.DTOs.Section
{
    public class SectionAnalyzeDTO
    {
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public int Access {get;set;}
        public int Done { get; set; }
        public virtual ICollection<ScriptAnalyzeDTO> Scripts { get; set; }
    }
}