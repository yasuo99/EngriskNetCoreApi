using System;
using System.Collections.Generic;
using Application.DTOs.Section;
using Domain.Enums;

namespace Application.DTOs.Account.Route
{
    public class RouteDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RouteImage { get; set; }
        public bool IsPrivate { get; set; }
        public int Done { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string PublishStatus { get; set; }
        public List<SectionDTO> Sections { get; set; }
    }
}