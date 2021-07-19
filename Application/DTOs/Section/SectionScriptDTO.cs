using System;
using System.Collections.Generic;

namespace Application.DTOs.Section
{
    public class SectionScriptDTO
    {
        public Guid Id { get; set; }
        public string SectionName { get; set; }
        public List<ScriptLearnDTO> Scripts { get; set; }
    }
}