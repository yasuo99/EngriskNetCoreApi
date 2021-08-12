using System;

namespace Application.DTOs.Section
{
    public class ScriptAnalyzeDTO
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public int TotalWord { get; set; }
        public int TotalQuestion { get; set; }
        public int Access { get; set; }
        public int Done { get; set; }
    }
}