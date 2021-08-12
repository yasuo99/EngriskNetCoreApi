using System;
using System.Collections.Generic;
using Application.DTOs.Section;
using Domain.Models.Version2;

namespace Application.DTOs.Account.Route
{
    public class RouteAnalyzeDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RouteImage { get; set; }
        public bool IsSequentially { get; set; }
        public int TotalParticipate { get; set; }
        public int TotalDoneTime { get; set; }
        public List<AccountBlogDTO> Participates { get; set; }
        public Dictionary<int, string> Progresses { get; set; }
        public List<SectionAnalyzeDTO> Sections { get; set; }
    }
}