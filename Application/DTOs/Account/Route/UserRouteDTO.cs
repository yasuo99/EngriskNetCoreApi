using System;
using System.Collections.Generic;
using Application.DTOs.Section;

namespace Application.DTOs.Account.Route
{
    public class UserRouteDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string RouteImage { get; set; }
        public string Description { get; set; }
        public bool IsDoing { get; set; }
        public List<SectionDTO> Sections { get; set; }
    }
}