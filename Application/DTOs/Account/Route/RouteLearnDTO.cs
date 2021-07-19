using System;
using System.Collections.Generic;
using Application.DTOs.Section;

namespace Application.DTOs.Account.Route
{
    public class RouteLearnDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RouteImage { get; set; }
        public bool IsPrivate { get; set; }
        public int Done { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<SectionLearnDTO> Sections { get; set; }
    }
}