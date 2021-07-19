using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account.Route;
using Application.DTOs.Pagination;
using Application.DTOs.Quiz;
using Application.DTOs.Section;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class RouteService : IRouteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private Route _route;
        public RouteService(ApplicationDbContext context, IMapper mapper, IFileService fileService, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
            _notificationService = notificationService;
        }
        public async Task<PaginateDTO<RouteDTO>> AdminGetEngriskAllRouteAsync(PaginationDTO pagination, string search = null)
        {
            var routes = await _context.Routes.Where(route => route.AccountId == null).Include(inc => inc.Sections).ToListAsync();
            if (search != null)
            {
                routes = routes.Where(route => route.Title.ToLower().Contains(search.ToLower()) || route.Description.ToLower().Contains(search.ToLower())).ToList();
            }
            var routesDTO = _mapper.Map<List<RouteDTO>>(routes);
            var pagingListRoutes = PagingList<RouteDTO>.OnCreate(routesDTO, pagination.CurrentPage, pagination.PageSize);
            return pagingListRoutes.CreatePaginate();
        }

        public async Task<bool> ChangePrivateStatusAsync(Guid id)
        {
            _route ??= await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).FirstOrDefaultAsync();
            if (_route.Sections.Count > 0)
            {
                _route.IsPrivate = !_route.IsPrivate;
                if (_route.VerifiedStatus == Status.Pending)
                {
                    _route.VerifiedStatus = Status.Nope;
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        return true;
                    }
                    return false;
                }
                if (!_route.IsPrivate)
                {
                    _route.VerifiedStatus = Status.Pending;
                }
                else
                {
                    _route.VerifiedStatus = Status.Nope;
                }
                if (await _context.SaveChangesAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public async Task<bool> ChangeRouteStatusAsync(Guid id)
        {
            _route ??= await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).FirstOrDefaultAsync();
            _route.IsPrivate = !_route.IsPrivate;
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckRouteExistAsync(Guid id)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            return _route != null;
        }

        public async Task<bool> CheckRouteSequentiallyAsync(Guid id)
        {
            var route = await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            return route.IsSequentially;
        }

        public async Task<bool> CheckRouteOwnerAsync(Guid id, int accountId)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            return _route.AccountId == accountId;
        }

        public async Task<RouteDTO> CreateRouteAsync(RouteCreateDTO routeCreate)
        {
            var route = _mapper.Map<Route>(routeCreate);
            if (routeCreate.Image != null)
            {
                route.RouteImage = _fileService.UploadFile(routeCreate.Image, SD.ImagePath);
            }
            _context.Routes.Add(route);
            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<RouteDTO>(route);
            };
            return null;
        }

        public async Task DeleteRouteAsync(Guid id)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            _context.Remove(_route);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRouteAsync(Route route)
        {
            _context.Remove(route);
            await _context.SaveChangesAsync();
        }
        private async Task CreateAccountLearningProgress()
        {

        }

        public async Task<TypeRouteDTO> GetAllEngriskRouteAndProgressAsync(int accountId)
        {
            var typeRoutes = new TypeRouteDTO();
            var engriskRoutes = new List<RouteLearnDTO>();
            var privateRoutes = new List<RouteLearnDTO>();
            // var sectionProgress = await _context.SectionProgresses.Where(sec => sec.AccountId == accountId && sec.Section.RouteId != null).Include(inc => inc.Section).AsNoTracking().ToListAsync();
            // var grouped = sectionProgress.GroupBy(groupBy => groupBy.Section.RouteId).ToDictionary(key => key.Key, value => value.Count(count => count.IsDone));
            // var routesProgress = grouped.OrderByDescending(orderBy => orderBy.Value).Select(sel => sel.Key);
            var allRoutes = await _context.Routes.Where(route => route.AccountId == null || route.AccountId == accountId).Include(inc => inc.Sections).AsNoTracking().ToListAsync();
            foreach (var route in allRoutes)//Classify route
            {
                var routeDto = _mapper.Map<RouteLearnDTO>(route);
                routeDto.Sections = routeDto.Sections.OrderBy(orderBy => orderBy.Index).ToList();
                foreach (var section in routeDto.Sections)
                {
                    var sectionProgress = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.SectionId == section.Id).Include(inc => inc.Details).FirstOrDefaultAsync();  //Check is route has progress history
                    if (sectionProgress != null)//If yes then set props with progress props
                    {
                        section.IsDone = sectionProgress.IsDone;
                        section.IsCurrentLocked = sectionProgress.IsLock;
                    }
                    else
                    {
                        section.IsDone = false;
                        section.IsCurrentLocked = section.Index == 0 ? false : routeDto.Sections[section.Index - 1].IsDone ? false : true;
                    }
                    var sectionScripts = await _context.Scripts.Where(script => script.SectionId == section.Id).OrderBy(orderBy => orderBy.Index).ToListAsync();
                    foreach (var script in sectionScripts)
                    {
                        var scriptProgress = sectionProgress?.Details.FirstOrDefault(spd => spd.ScriptId == script.Id);
                        section.Scripts.Add(new ScriptLearnHistoryDTO
                        {
                            Id = script.Id,
                            Type = Enum.GetName(typeof(ScriptTypes), script.Type),
                            IsDone = scriptProgress != null ? scriptProgress.IsDone : false,
                        });
                    }
                    var undoneFirstScript = section.Scripts.FirstOrDefault(script => !script.IsDone);
                    if (undoneFirstScript != null)
                    {
                        if (!section.IsCurrentLocked)
                        {
                            undoneFirstScript.IsCurrentPosition = true;
                        }
                    }
                    var doneCount = section.Scripts.Where(script => script.IsDone).Count();
                    section.DonePercent = sectionScripts.Count > 0 ? Math.Round((double)((double)doneCount / (double)sectionScripts.Count) * 100) : 0;
                }
                if (route.AccountId == null)
                {
                    engriskRoutes.Add(routeDto);
                }
                if (route.AccountId == accountId)
                {
                    privateRoutes.Add(routeDto);
                }
            }
            typeRoutes.Engrisk = engriskRoutes;
            typeRoutes.Private = privateRoutes;
            var lastRoute = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.IsLastDoing).Include(inc => inc.Section).FirstOrDefaultAsync();
            if (lastRoute != null)
            {
                var isEngriskRoute = engriskRoutes.Any(route => route.Id == lastRoute.Section.RouteId);
                if (isEngriskRoute)
                {
                    typeRoutes.LastRoute = _mapper.Map<RouteLearnDTO>(engriskRoutes.FirstOrDefault(route => route.Id == lastRoute.Section.RouteId));
                }
                else
                {
                    typeRoutes.LastRoute = _mapper.Map<RouteLearnDTO>(privateRoutes.FirstOrDefault(route => route.Id == lastRoute.Section.RouteId));
                }
            }
            return typeRoutes;
        }

        public async Task<PaginateDTO<RouteDTO>> GetAllUserRoute(PaginationDTO pagination, int accountId, bool isPrivate = true, Status status = Status.Approved)
        {
            var routes = await _context.Routes.Where(route => route.AccountId == accountId && route.VerifiedStatus == status && route.IsPrivate == isPrivate).Include(inc => inc.Sections).ToListAsync();
            var routesDto = _mapper.Map<List<RouteDTO>>(routes);
            var pagingListRoutes = PagingList<RouteDTO>.OnCreate(routesDto, pagination.CurrentPage, pagination.PageSize);
            return pagingListRoutes.CreatePaginate();
        }

        public async Task<TypeRouteDTO> GetAnonymousRouteAsync()
        {
            var engriskRoutes = await _context.Routes.Where(route => (route.AccountId == null)).Include(inc => inc.Sections).ToListAsync();
            var engriskRoutesDto = _mapper.Map<List<RouteLearnDTO>>(engriskRoutes);
            var typeRoute = new TypeRouteDTO();
            foreach (var routeDto in engriskRoutesDto)
            {
                var route = engriskRoutes.FirstOrDefault(route => route.Id == routeDto.Id);
                routeDto.Sections = _mapper.Map<List<SectionLearnDTO>>(route.Sections.OrderBy(orderBy => orderBy.Index).ToList());
                foreach (var section in routeDto.Sections)
                {
                    var sectionScripts = await _context.Scripts.Where(script => script.SectionId == section.Id).ToListAsync();
                    foreach (var script in sectionScripts)
                    {
                        section.Scripts.Add(new ScriptLearnHistoryDTO
                        {
                            Id = script.Id,
                            Type = Enum.GetName(typeof(ScriptTypes), script.Type),
                            IsDone = false
                        });
                    }

                    section.IsDone = false;
                    if (section.Index == 0)
                    {
                        section.IsCurrentLocked = false;

                    }
                    else
                    {
                        section.IsCurrentLocked = true;
                    }

                }
                if (route.AccountId == null)
                {
                    typeRoute.Engrisk.Add(routeDto);
                }
            }
            return typeRoute;
        }

        public async Task<List<RouteDTO>> GetEngriskAllRouteAsync()
        {
            var routes = await _context.Routes.Where(route => route.AccountId == null).ToListAsync();
            return _mapper.Map<List<RouteDTO>>(routes);
        }

        public async Task<RouteDTO> GetNearestFinishRouteAsync(int accountId)
        {
            var sectionProgress = await _context.SectionProgresses.Where(sec => sec.AccountId == accountId).Include(inc => inc.Section).ToListAsync();
            var grouped = sectionProgress.GroupBy(groupBy => groupBy.Section.RouteId).ToDictionary(key => key.Key, value => value.Count(count => count.IsDone));
            var toponeRouteId = grouped.OrderByDescending(orderBy => orderBy.Value).FirstOrDefault().Key;
            var route = await _context.Routes.Where(route => route.Id == toponeRouteId).Include(inc => inc.Sections).FirstOrDefaultAsync();
            var routeDto = _mapper.Map<RouteDTO>(route);
            foreach (var section in routeDto.Sections)
            {
                var sectionStatus = await _context.SectionProgresses.Where(sp => sp.SectionId == section.Id && sp.AccountId == accountId).FirstOrDefaultAsync();
                section.IsDone = sectionStatus.IsDone;
                // var wordLearnt = await _context.WordLearnts.Where(w => w.AccountId == accountId).Include(inc => inc.Word).ThenInclude(inc => inc.WordCategory).Select(sel => sel.Word).CountAsync(w => w.WordCategory.SectionId == section.Id);
            }
            return routeDto;
        }

        public async Task<RouteDTO> GetRouteDetailAsync(Guid id)
        {
            _route ??= await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).FirstOrDefaultAsync();
            _route.Sections = _route.Sections.OrderBy(orderBy => orderBy.Index).ToList();
            var route = _mapper.Map<RouteDTO>(_route);
            return route;
        }

        public async Task<RouteDTO> GetRouteProgressAsync(Guid id, int accountId)
        {
            _route ??= await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).ThenInclude(inc => inc.Scripts).FirstOrDefaultAsync();
            var route = _mapper.Map<RouteDTO>(_route);
            foreach (var section in route.Sections)
            {
                var sectionStatus = await _context.SectionProgresses.Where(sp => sp.SectionId == section.Id && sp.AccountId == accountId).Include(inc => inc.Details).FirstOrDefaultAsync();
                section.IsDone = sectionStatus.IsDone;
                section.IsCurrentLocked = sectionStatus.IsLock;
                // var wordLearnt = await _context.WordLearnts.Where(w => w.AccountId == accountId).Include(inc => inc.Word).ThenInclude(inc => inc.WordCategory).Select(sel => sel.Word).CountAsync(w => w.WordCategory.SectionId == section.Id);
                section.Scripts = _mapper.Map<List<ScriptLearnHistoryDTO>>(sectionStatus.Details);
            }
            return route;
        }
        public async Task<bool> ReArrangeSectionsRouteAsync(Guid id, List<Guid> sections)
        {
            var route = await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).FirstOrDefaultAsync();
            var removeSections = route.Sections.Where(section => !sections.Any(sectionId => sectionId == section.Id)).Select(sel => sel.Id).ToList();
            foreach (var removeSectionId in removeSections)
            {
                var section = route.Sections.FirstOrDefault(sec => sec.Id == removeSectionId);
                route.Sections.Remove(section);
            }
            for (var i = 0; i < sections.Count; i++)
            {
                var section = route.Sections.FirstOrDefault(sec => sec.Id == sections[i]);
                if (section != null)
                {
                    section.Index = i;
                }
                else
                {
                    var sectionFromDb = await _context.Sections.FirstOrDefaultAsync(sec => sec.Id == sections[i]);
                    sectionFromDb.Index = i;
                    route.Sections.Add(sectionFromDb);
                }
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<RouteDTO> UpdateRouteAsync(Guid id, RouteUpdateDTO routeUpdate)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            _mapper.Map(routeUpdate, _route);
            if (routeUpdate.Image != null)
            {
                if (!string.IsNullOrEmpty(_route.RouteImage))
                {
                    _fileService.DeleteFile(_route.RouteImage);
                }
                _route.RouteImage = _fileService.UploadFile(routeUpdate.Image, SD.ImagePath);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<RouteDTO>(_route);
            }
            return null;
        }

        public async Task<bool> CheckSectionExistAsync(Guid id, Guid sectionId)
        {
            _route = await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).FirstOrDefaultAsync();
            return _route.Sections.Any(sec => sec.Id == sectionId);
        }

        public async Task<bool> CheckAnonymousSectionAsync(Guid routeId, Guid sectionId)
        {
            var route = await _context.Routes.Where(route => route.Id == routeId).Include(inc => inc.Sections).FirstOrDefaultAsync();
            return route.Sections.Where(section => section.Id == sectionId).FirstOrDefault().Index == 0;
        }
    }
}