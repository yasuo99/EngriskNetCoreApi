using System;

namespace Application.DTOs.Section
{
    public class ScriptDoneDTO
    {
        public Guid ScriptId { get; set; }
        public Guid SectionId { get; set; }
        public int AccountId { get; set; }
    }
}