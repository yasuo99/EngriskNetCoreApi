using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.Account.Route;
using Application.DTOs.Certificate;
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
        private readonly ICertificateService _certificateService;
        private Route _route;
        public RouteService(ApplicationDbContext context, IMapper mapper, IFileService fileService, INotificationService notificationService, ICertificateService certificateService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
            _notificationService = notificationService;
            _certificateService = certificateService;
        }
        public async Task<PaginateDTO<RouteDTO>> AdminGetEngriskAllRouteAsync(PaginationDTO pagination, PublishStatus status = PublishStatus.None, string search = null)
        {
            var routes = from r in _context.Routes.OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking() select r;
            if (status != PublishStatus.None)
            {
                routes = routes.Where(route => route.PublishStatus == status);
            }
            if (search != null)
            {
                routes = routes.Where(route => route.Title.ToLower().Contains(search.ToLower()) || route.Description.ToLower().Contains(search.ToLower()));
            }
            var pagingListRoutes = await PagingList<Route>.OnCreateAsync(routes, pagination.CurrentPage, pagination.PageSize);
            var result = pagingListRoutes.CreatePaginate();
            var routesDTO = _mapper.Map<List<RouteDTO>>(result.Items);
            foreach(var route in routesDTO){
                route.Sections = _mapper.Map<List<SectionDTO>>(await _context.Sections.Where(sec => sec.RouteId == route.Id).ToListAsync());
            }
            return new PaginateDTO<RouteDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Items = routesDTO,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
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
            var allRoutes = await _context.Routes.Where(route => route.PublishStatus == PublishStatus.Published).Include(inc => inc.Sections).AsNoTracking().ToListAsync();
            foreach (var route in allRoutes)//Classify route
            {
                route.Sections = route.Sections.Where(sec => sec.PublishStatus == PublishStatus.Published).ToList();
                var routeDto = _mapper.Map<RouteLearnDTO>(route);
                routeDto.Sections = routeDto.Sections.OrderBy(orderBy => orderBy.Index).ToList();
                foreach (var section in routeDto.Sections)
                {
                    var sectionProgress = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.SectionId == section.Id).Include(inc => inc.Details).FirstOrDefaultAsync();  //Check is route has progress history
                    section.IsDone = sectionProgress != null ? sectionProgress.IsDone : false;
                    if (route.IsSequentially)
                    { //Check is that route sequentially or not, if true make sure user done previous section before go next
                        if (sectionProgress != null)//If yes then set props with progress props
                        {
                            section.IsCurrentLocked = sectionProgress.IsLock;
                        }
                        else
                        {
                            section.IsCurrentLocked = section.Index == 0 ? false : routeDto.Sections[section.Index - 1].IsDone ? false : true;
                        }
                    }
                    else
                    {
                        section.IsCurrentLocked = false;
                    }

                    var sectionScripts = await _context.Scripts.Where(script => script.SectionId == section.Id).Include(inc => inc.MiniExam).OrderBy(orderBy => orderBy.Index).ToListAsync();
                    foreach (var script in sectionScripts)
                    {
                        var scriptProgress = sectionProgress?.Details.FirstOrDefault(spd => spd.ScriptId == script.Id);
                        section.Scripts.Add(new ScriptLearnHistoryDTO
                        {
                            Id = script.Id,
                            Type = Enum.GetName(typeof(ScriptTypes), script.Type),
                            IsDone = scriptProgress != null ? scriptProgress.IsDone : false,
                            ExamId = script.MiniExam != null ? script.MiniExam.Id : Guid.Empty
                        });
                    }
                    var undoneFirstScript = section.Scripts.FirstOrDefault(script => !script.IsDone);
                    if (undoneFirstScript != null)
                    {
                        if (!section.IsCurrentLocked)
                        {
                            if (!route.IsSequentially)
                            {
                                if (!routeDto.Sections.Any(sec => sec.Scripts.Any(script => script.IsCurrentPosition)))
                                {
                                    undoneFirstScript.IsCurrentPosition = true;
                                }
                            }
                            else
                            {
                                undoneFirstScript.IsCurrentPosition = true;
                            }
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
            var engriskRoutes = await _context.Routes.Where(route => route.PublishStatus == PublishStatus.Published).Include(inc => inc.Sections).ToListAsync();
            var engriskRoutesDto = _mapper.Map<List<RouteLearnDTO>>(engriskRoutes);
            var typeRoute = new TypeRouteDTO();
            foreach (var routeDto in engriskRoutesDto)
            {
                var route = engriskRoutes.FirstOrDefault(route => route.Id == routeDto.Id);
                routeDto.Sections = _mapper.Map<List<SectionLearnDTO>>(route.Sections.Where(sec => sec.PublishStatus == PublishStatus.Published).OrderBy(orderBy => orderBy.Index).ToList());
                foreach (var section in routeDto.Sections)
                {
                    var sectionScripts = await _context.Scripts.Where(script => script.SectionId == section.Id).ToListAsync();
                    foreach (var script in sectionScripts)
                    {
                        section.Scripts.Add(new ScriptLearnHistoryDTO
                        {
                            Id = script.Id,
                            Type = Enum.GetName(typeof(ScriptTypes), script.Type),
                            IsDone = false,
                            Index = script.Index
                        });
                    }

                    section.IsDone = false;
                    if (section.Index == 0)
                    {
                        section.IsCurrentLocked = false;
                        if (section.Scripts.Count > 0)
                        {
                            section.Scripts.OrderBy(orderby => orderby.Index).FirstOrDefault().IsCurrentPosition = true;
                        }
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

        public async Task<bool> SelectRouteAsync(Guid routeId, int accountId)
        {
            var routeProgress = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId).Include(inc => inc.Section).ToListAsync();
            routeProgress.ForEach((route) =>
            {
                if (route.Section.RouteId == routeId)
                {
                    route.IsLastDoing = true;
                }
                else
                {
                    route.IsLastDoing = false;
                }
            });
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task PublishAsync(Guid id, PublishStatus status)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
            _route.PublishStatus = status;
            await _notificationService.SendSignalRResponse("Refresh", "");
            await _context.SaveChangesAsync();
            // if(status == PublishStatus.Published){
            //     _route = await _context.Routes.Where(route => route.Id == id).Include()
            // }
        }

        public async Task<CertificateRequestResponseDTO> RequestCertificateAsync(int accountId, Guid routeId)
        {
            var certificateScript = await _context.Scripts.Where(script => script.Section.RouteId == routeId && script.Type == ScriptTypes.Certificate).Include(inc => inc.MiniExam).Include(inc => inc.Certificate).AsNoTracking().FirstOrDefaultAsync();
            if (certificateScript == null)
            {
                return new CertificateRequestResponseDTO
                {
                    Status = 404,
                    Content = "Lộ trình học này không có chứng chỉ",
                };
            }
            var examResult = await _context.ExamHistories.Where(eh => eh.AccountId == accountId && eh.ExamId == certificateScript.MiniExam.Id && eh.Score >= certificateScript.MiniExam.PassScore && !eh.ReceivedCertificate).OrderByDescending(orderBy => orderBy.Timestamp_end).AsNoTracking().FirstOrDefaultAsync();
            if (examResult == null)
            {
                return new CertificateRequestResponseDTO
                {
                    Status = 404,
                    Content = "Bạn chưa đủ điều kiện để nhận chứng chỉ này"
                };
            }
            var certificate = await _context.AccountCertificates.AsNoTracking().FirstOrDefaultAsync(ac => ac.AccountId == accountId && ac.CertificateId == certificateScript.Certificate.Id);
            if (certificate == null)
            {
                return new CertificateRequestResponseDTO
                {
                    Status = 200,
                    Result = true,
                    Content = "Bạn đủ điều kiện để nhận chứng chỉ này"
                };
            }
            if (certificate.ExpireDate < DateTime.Now)
            {
                return new CertificateRequestResponseDTO
                {
                    Status = 200,
                    Result = true,
                    Content = "Bạn đủ điều kiện để nhận chứng chỉ này"
                }; ;
            }
            return new CertificateRequestResponseDTO
            {
                Status = 404,
                Content = "Chứng chỉ hiện tại của bạn vẫn chưa hết hạn."
            }; ;
        }

        public async Task<bool> CheckRouteStatusAsync(Guid routeId)
        {
            _route ??= await _context.Routes.FirstOrDefaultAsync(route => route.Id == routeId);
            return _route.PublishStatus == PublishStatus.Published;
        }

        public async Task<bool> CheckSectionStatusAsync(Guid routeId, Guid sectionId)
        {
            _route ??= await _context.Routes.Include(inc => inc.Sections).FirstOrDefaultAsync(route => route.Id == routeId);
            var section = _route.Sections.FirstOrDefault(section => section.Id == sectionId);
            return section.PublishStatus == PublishStatus.Published;
        }

        public async Task<string> ClaimCertificateAsync(int accountId, Guid routeId, SignatureCertificateDTO signatureCertificateDTO)
        {
            var certificateScript = await _context.Scripts.Where(script => script.Section.RouteId == routeId && script.Type == ScriptTypes.Certificate).Include(inc => inc.MiniExam).Include(inc => inc.Certificate).AsNoTracking().FirstOrDefaultAsync();
            var examResult = await _context.ExamHistories.Where(eh => eh.AccountId == accountId && eh.ExamId == certificateScript.MiniExam.Id && eh.Score >= certificateScript.MiniExam.PassScore && !eh.ReceivedCertificate).OrderByDescending(orderBy => orderBy.Timestamp_end).FirstOrDefaultAsync();
            signatureCertificateDTO.Score = examResult.Score;
            var certificate = await _certificateService.CreateUserCertificateAsync(accountId, certificateScript.Certificate.Id, signatureCertificateDTO);
            var account = await _context.Accounts.Where(acc => acc.Id == accountId).Include(inc => inc.Certificates).FirstOrDefaultAsync();
            account.Certificates.Add(new AccountCertificate
            {
                CertificateId = certificateScript.Certificate.Id,
                Signature = certificate,
                AchievedDate = DateTime.Now.Date,
                ExpireDate = DateTime.Now.Date.AddMonths(certificateScript.Certificate.LifeTime)
            });
            examResult.ReceivedCertificate = true;
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task<RouteOverviewDTO> GetRouteOverviewAsync(DateRangeDTO dateRange)
        {
            var routeOverviewDTO = new RouteOverviewDTO();
            var sectionProgresses = await _context.SectionProgresses.Include(inc => inc.Section).ToListAsync();
            routeOverviewDTO.TotalParticipate = sectionProgresses.Select(sel => sel.AccountId).Distinct().Count(); //Đếm số lượng học viên

            var temp2 = sectionProgresses.GroupBy(group => group.Section.RouteId).ToDictionary(key => key.Key, value => value.Select(sel => new { sectionId = sel.SectionId, IsDone = sel.IsDone }).Distinct().Count(sp => sp.IsDone));
            var routes = await _context.Routes.Include(inc => inc.Sections).ToListAsync();
            routeOverviewDTO.TotalSection = routes.SelectMany(sel => sel.Sections).Count();
            routeOverviewDTO.TotalRoute = routes.Count();
            routeOverviewDTO.TotalDone = temp2.Where(temp => routes.FirstOrDefault(route => route.Id == temp.Key).Sections.Where(sec => sec.PublishStatus == PublishStatus.Published).Count() == temp.Value).Count();
            var temp = sectionProgresses.GroupBy(group => group.AccountId).ToDictionary(key => key.Key, data => data.GroupBy(group => group.Section.RouteId));
            var test = temp.ToDictionary(key => key.Key, key => key.Value.Count(val => val.Count(count => count.IsDone) == routes.FirstOrDefault(route => route.Id == val.Key).Sections.Where(sec => sec.PublishStatus == PublishStatus.Published).Count()));
            var dayStudies = await _context.DayStudies.Where(ds => ds.TotalSections > 0).Include(inc => inc.Account).ToListAsync();
            routeOverviewDTO.Progress = dayStudies.GroupBy(group => group.Date).ToDictionary(key => key.Key, value => value.Sum(sum => sum.TotalSections));
            routeOverviewDTO.Routes = _mapper.Map<List<RouteAnalyzeDTO>>(routes);
            return routeOverviewDTO;
        }

        public async Task<RouteAnalyzeDTO> GetRouteAnalyzeAsync(Guid id)
        {
            _route = await _context.Routes.Where(route => route.Id == id).Include(inc => inc.Sections).ThenInclude(inc => inc.Scripts).FirstOrDefaultAsync();
            var routeAnalyze = _mapper.Map<RouteAnalyzeDTO>(_route);
            foreach (var section in routeAnalyze.Sections)
            {
                var sectionProgress = await _context.SectionProgresses.Where(sp => sp.SectionId == section.Id).ToListAsync();
                section.Access = sectionProgress.Count;
                section.Done = sectionProgress.Count(count => count.IsDone);
                foreach (var script in section.Scripts)
                {
                    var sectionDetailProgresses = await _context.SectionDetailProgresses.Where(sdp => sdp.ScriptId == script.Id).ToListAsync();
                    script.Done = sectionDetailProgresses.Count(sdp => sdp.IsDone);
                    script.Access = sectionDetailProgresses.Count();
                }
            }
            var routeProgress = await _context.SectionProgresses.Where(sp => sp.Section.RouteId == id).Include(inc => inc.Section).Include(inc => inc.Account).ToListAsync();
            var temp = routeProgress.GroupBy(group => group.AccountId).ToDictionary(dic => dic.Key, dic => dic.Where(rp => rp.IsDone));
            routeAnalyze.TotalDoneTime = temp.Count(count => count.Value.Count() == _route.Sections.Count);
            routeAnalyze.TotalParticipate = routeProgress.Select(sel => sel.AccountId).Distinct().Count();
            routeAnalyze.Participates = _mapper.Map<List<AccountBlogDTO>>(routeProgress.Select(sel => sel.Account).Distinct().ToList());
            routeAnalyze.Progresses = routeProgress.GroupBy(group => group.AccountId).ToDictionary(key => key.Key, data => data.ToList().CamelcaseSerialize());
            return routeAnalyze;
        }
    }
}