using System;
using System.Collections.Generic;

namespace Application.DTOs.Section
{
    public class SectionScriptDTO
    {
        public Guid Id { get; set; }
        public string SectionName { get; set; }
        public int Index { get; set; }

        public List<ScriptLearnDTO> Scripts { get; set; }
        public bool CertificateScriptAvailable
        {
            get
            {
                return Index >= 5; //Xét điều kiện cho phép 
            }
        }
    }
}