using System;
using System.Collections.Generic;

namespace Application.DTOs.Account.Route
{
    public class RouteOverviewDTO
    {
        public int TotalRoute { get; set; }
        public int TotalParticipate { get; set; }
        public int TotalSection { get; set; }
        public int TotalDone { get; set; }
        public Dictionary<string,int> DonePie { get; set; }
        public Dictionary<DateTime,int> Progress { get; set; }
        public List<RouteAnalyzeDTO> Routes { get; set; }

    }
}