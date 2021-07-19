using System;
using System.Collections.Generic;
using Application.DTOs.Section;

namespace Application.DTOs.Account.Route
{
    public class RouteCensorDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RouteImage { get; set; }
        public virtual AccountBlogDTO Account { get; set; }
        public virtual ICollection<SectionDTO> Sections { get; set; }
    }
}