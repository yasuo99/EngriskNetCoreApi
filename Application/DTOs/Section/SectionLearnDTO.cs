using System;
using System.Collections.Generic;
using Application.DTOs.Account.Route;

namespace Application.DTOs.Section
{
    public class SectionLearnDTO
    {
        public SectionLearnDTO()
        {
            Scripts = new List<ScriptLearnHistoryDTO>();
        }
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public Guid RouteId { get; set; }
        public int Index { get; set; }
        public double DonePercent { get; set; }
        public bool IsDone { get; set; }
        public bool IsCurrentLocked { get; set; }
        public bool RequireLogin { get; set; }
        public int TotalWord { get; set; }
        public List<ScriptLearnHistoryDTO> Scripts { get; set; }
    }
}