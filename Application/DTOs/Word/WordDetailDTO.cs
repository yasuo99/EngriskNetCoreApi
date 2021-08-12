using System;
using System.Collections.Generic;
using Application.DTOs.Example;

namespace Application.DTOs.Word
{
    public class WordDetailDTO
    {
        public Guid Id { get; set; }
        public string WordCategory { get; set; }
        public string WordImg { get; set; }
        public string Eng  { get; set; }
        public string Spelling { get; set; }
        public string WordVoice { get; set; }
        public string Vie { get; set; }
        public string Class { get; set; }
        public virtual IEnumerable<ExampleDTO> Examples{get;set;}
    }
}