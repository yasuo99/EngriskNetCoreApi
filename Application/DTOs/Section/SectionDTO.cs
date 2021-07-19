using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Application.DTOs.Account.Route;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Section
{
    public class SectionDTO
    {
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public virtual RouteDTO Route { get; set; }
        public int Index { get; set; }
        public bool IsDone { get; set; }
        public bool IsCurrentLocked { get; set; }
        public bool RequireLogin { get; set; }
        public int TotalWord { get; set; }
        public List<ScriptLearnHistoryDTO> Scripts { get; set; }
    }
}