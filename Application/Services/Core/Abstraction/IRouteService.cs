using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Account.Route;
using Application.DTOs.Admin;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Application.DTOs.Section;
using Domain.Enums;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface IRouteService : IPublishService
    {
        Task<bool> CheckRouteStatusAsync(Guid routeId);
        Task<bool> CheckSectionStatusAsync(Guid routeId, Guid sectionId);
        Task<bool> CheckAnonymousSectionAsync(Guid routeId, Guid sectionId);
        Task<bool> SelectRouteAsync(Guid routeId, int accountId);
        Task<bool> CheckRouteExistAsync(Guid id);
        Task<bool> CheckSectionExistAsync(Guid id, Guid sectionId);
        Task<bool> CheckRouteSequentiallyAsync(Guid id);
        Task<List<RouteDTO>> GetEngriskAllRouteAsync();
        Task<TypeRouteDTO> GetAllEngriskRouteAndProgressAsync(int accountId);
        Task<PaginateDTO<RouteDTO>> GetAllUserRoute(PaginationDTO pagination, int accountId, bool isPrivate = true, Status status = Status.Nope);
        Task<RouteOverviewDTO> GetRouteOverviewAsync(DateRangeDTO dateRangeDTO);
        Task<RouteAnalyzeDTO> GetRouteAnalyzeAsync(Guid id);
        Task<bool> CheckRouteOwnerAsync(Guid id, int accountId);
        Task<bool> ChangePrivateStatusAsync(Guid id);
        Task<PaginateDTO<RouteDTO>> AdminGetEngriskAllRouteAsync(PaginationDTO pagination, PublishStatus publishStatus = PublishStatus.None, string search = null);
        Task<RouteDTO> GetRouteDetailAsync(Guid id);
        Task<bool> ChangeRouteStatusAsync(Guid id);
        Task<CertificateRequestResponseDTO> RequestCertificateAsync(int accountId, Guid routeId);
        Task<string> ClaimCertificateAsync(int accountId, Guid routeId, SignatureCertificateDTO signatureCertificateDTO);
        Task<RouteDTO> GetNearestFinishRouteAsync(int accountId);
        Task<TypeRouteDTO> GetAnonymousRouteAsync();
        Task<RouteDTO> GetRouteProgressAsync(Guid id, int accountId);
        Task<RouteDTO> CreateRouteAsync(RouteCreateDTO routeCreate);
        Task<RouteDTO> UpdateRouteAsync(Guid id, RouteUpdateDTO routeUpdate);
        Task<bool> ReArrangeSectionsRouteAsync(Guid id, List<Guid> sections);
        Task DeleteRouteAsync(Guid id);
        Task DeleteRouteAsync(Route route);
    }
}