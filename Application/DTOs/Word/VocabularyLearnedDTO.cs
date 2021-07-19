using System;

namespace Application.DTOs.Word
{
    public class VocabularyLearnedDTO
    {
        public Guid Id { get; set; }
        public string Eng { get; set; }
        public string Vie { get; set; }
        public string LearnStatus{get;set;}
    }
}