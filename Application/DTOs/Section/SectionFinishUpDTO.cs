using System;
using System.Collections.Generic;
using Domain.Models.Version2;

namespace Application.DTOs.Section
{
    public class SectionFinishUpDTO
    {
        public SectionFinishUpDTO()
        {
            DayStudied = new Dictionary<DateTime, bool>();
        }
        public int SectionDone { get; set; }
        public int TotalSection { get; set; }
        public int LearnedVocabulary { get; set; }
        public int Active_Days { get; set; }
        public Dictionary<DateTime,bool> DayStudied { get; set; }
    }
}