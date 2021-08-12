using System.Collections.Generic;
using Application.DTOs.Section;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Account.Route
{
    public class RouteUpdateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public PublishStatus PublishStatus { get; set; } = PublishStatus.UnPublished;
        public List<SectionDTO> Sections { get; set; }
    }
}