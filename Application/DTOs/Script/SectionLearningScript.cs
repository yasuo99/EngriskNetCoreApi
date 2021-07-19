using System.Collections.Generic;

namespace Application.DTOs.Script
{
    public class SectionLearningScript
    {
        public SectionLearningScript()
        {
            Scripts = new List<ScriptDTO>();
        }
        public List<ScriptDTO> Scripts { get; set; }
    }
}