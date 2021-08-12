using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Account.Route;
using Application.DTOs.Admin;
using Application.DTOs.Exam;
using Application.DTOs.Example;
using Application.DTOs.Memory;
using Application.DTOs.Pagination;
using Application.DTOs.Quiz;
using Domain.Enums;

namespace Application.Services.Core.Abstraction
{
    public interface IAdminService
    {
        //Get
        Task<dynamic> GetWaitingCensorContentAsync(PaginationDTO pagination, CensorTypes type);
        Task<DashboardDTO> GetDashboardAsync();
        //Put
        Task<bool> CensoredContentAsync(Guid id, CensorTypes type, Status status, DifficultLevel difficultLevel = DifficultLevel.Easy);
        Task<List<RouteOverviewDTO>> GetRouteOverviewAsync();
    }
}