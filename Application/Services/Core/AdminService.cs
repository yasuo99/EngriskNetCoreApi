using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.Account.Route;
using Application.DTOs.Admin;
using Application.DTOs.Admin.Chart;
using Application.DTOs.Exam;
using Application.DTOs.Example;
using Application.DTOs.Memory;
using Application.DTOs.Pagination;
using Application.DTOs.Quiz;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AdminService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<bool> CensoredContentAsync(Guid id, CensorTypes type, Status status, DifficultLevel difficultLevel = DifficultLevel.Easy)
        {
            switch (type)
            {
                case CensorTypes.Quiz:
                    var quiz = await _context.Quiz.FirstOrDefaultAsync(quiz => quiz.Id == id);
                    quiz.DifficultLevel = difficultLevel;
                    quiz.VerifiedStatus = status;
                    break;
                case CensorTypes.Exam:
                    var exam = await _context.Exam.FirstOrDefaultAsync(exam => exam.Id == id);
                    exam.Difficult = difficultLevel;
                    exam.VerifiedStatus = status;
                    break;
                case CensorTypes.Example:
                    var example = await _context.Examples.FirstOrDefaultAsync(example => example.Id == id);
                    example.VerifiedStatus = status;
                    break;
                case CensorTypes.Memory:
                    var memory = await _context.Memories.FirstOrDefaultAsync(memory => memory.Id == id);
                    memory.VerifiedStatus = status;
                    break;
                case CensorTypes.Post:
                    var post = await _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
                    post.VerifiedStatus = status;
                    break;
                case CensorTypes.Comment:
                    var comment = await _context.Comments.FirstOrDefaultAsync(comment => comment.Id == id);
                    comment.VerifiedStatus = status;
                    break;
                case CensorTypes.Route:
                    var route = await _context.Routes.FirstOrDefaultAsync(route => route.Id == id);
                    route.VerifiedStatus = status;
                    break;
                default:
                    break;
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<DashboardDTO> GetDashboardAsync()
        {
            var dashboard = new DashboardDTO();
            var accounts = await _context.Accounts.AsNoTracking().Where(acc => acc.IsDisabled == false).ToListAsync();
            dashboard.TotalAccount = accounts.Count;
            dashboard.JoinProgress = accounts.GroupBy(groupBy => groupBy.JoinedDate).Select(sel => new SeriesChartDTO { Date = sel.Key, Value = sel.Count() }).ToList();
            dashboard.TotalExam = await _context.Exam.AsNoTracking().Where(quiz => quiz.VerifiedStatus == Status.Nope).CountAsync();
            dashboard.TotalQuiz = await _context.Quiz.AsNoTracking().Where(quiz => quiz.VerifiedStatus == Status.Nope).CountAsync();
            dashboard.TotalRoute = await _context.Routes.Where(route => route.VerifiedStatus == Status.Nope).AsNoTracking().CountAsync();
            dashboard.Online = HubHelper.NotificationClientsConnections.Select(sel => sel.Username).ToList();
            return dashboard;
        }

        public async Task<List<RouteOverviewDTO>> GetRouteOverviewAsync()
        {
            List<RouteOverviewDTO> routeOverviewDTOs = new List<RouteOverviewDTO>();
            // var routes = await _context.Routes.Include(inc => inc.Sections).AsNoTracking().ToListAsync();
            // foreach (var route in routes)
            // {
            //     var totalParticipate = await _context.SectionProgresses.Where(sp => sp.Section.RouteId == route.Id).GroupBy(groupBy => groupBy.Section.RouteId).ToDictionaryAsync(dic => dic.Key, value => value.Count());
            //     // var doneRoute = await _context.SectionProgresses.Where(sp => sp.Section.RouteId == route.Id && sp.IsDone)
            //     var routeOverview = new RouteOverviewDTO
            //     {
            //         Route = route,
            //         TotalParticipate = totalParticipate.Count,

            //     };
            // }
            return routeOverviewDTOs;
        }

        public async Task<dynamic> GetWaitingCensorContentAsync(PaginationDTO pagination, CensorTypes type)
        {
            switch (type)
            {
                case CensorTypes.Quiz:
                    var quizzes = await _context.Quiz.Where(quiz => quiz.VerifiedStatus == Status.Pending).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).AsNoTracking().ToListAsync();
                    var quizzesDto = _mapper.Map<List<QuizDTO>>(quizzes);
                    foreach (var quiz in quizzesDto)
                    {
                        quiz.Owner = _mapper.Map<AccountBlogDTO>(await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(acc => acc.UserName.Equals(quiz.CreatedBy)));
                    }
                    var pagingListQuizzesDto = PagingList<QuizDTO>.OnCreate(quizzesDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListQuizzesDto.CreatePaginate();
                case CensorTypes.Exam:
                    var exams = await _context.Exam.Where(exam => exam.VerifiedStatus == Status.Pending).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).AsNoTracking().ToListAsync();
                    var examsDto = _mapper.Map<List<ExamDTO>>(exams);
                    foreach (var exam in examsDto)
                    {
                        exam.Owner = _mapper.Map<AccountBlogDTO>(await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(acc => acc.UserName.Equals(exam.CreatedBy)));
                    }
                    var pagingListExamsDto = PagingList<ExamDTO>.OnCreate(examsDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListExamsDto.CreatePaginate();
                case CensorTypes.Example:
                    var examples = await _context.ExampleContributors.Where(example => example.Example.VerifiedStatus == Status.Pending).Include(inc => inc.Example).Include(inc => inc.Account).AsNoTracking().ToListAsync();
                    var examplesDto = _mapper.Map<List<ExampleDTO>>(examples);
                    var pagingListExamplesDto = PagingList<ExampleDTO>.OnCreate(examplesDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListExamplesDto.CreatePaginate();
                case CensorTypes.Memory:
                    var memories = await _context.Memories.Where(memory => memory.VerifiedStatus == Status.Pending).Include(inc => inc.Account).Include(inc => inc.Word).AsNoTracking().ToListAsync();
                    var memoriesDto = _mapper.Map<List<MemoryCensorDTO>>(memories);
                    var pagingListmemoriesDto = PagingList<MemoryCensorDTO>.OnCreate(memoriesDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListmemoriesDto.CreatePaginate();
                case CensorTypes.Post:
                    var posts = await _context.Posts.Where(post => post.VerifiedStatus == Status.Pending).Include(inc => inc.Account).Include(inc => inc.PostImages).AsNoTracking().ToListAsync();
                    var postsDto = _mapper.Map<List<PostCensorDTO>>(posts);
                    var pagingListpostsDto = PagingList<PostCensorDTO>.OnCreate(postsDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListpostsDto.CreatePaginate();
                case CensorTypes.Comment:
                    var comments = await _context.Comments.Where(comment => comment.VerifiedStatus == Status.Pending).Include(inc => inc.Account).Include(inc => inc.Post).AsNoTracking().ToListAsync();
                    var commentsDto = _mapper.Map<List<CommentCensorDTO>>(comments);
                    var pagingListcommentsDto = PagingList<CommentCensorDTO>.OnCreate(commentsDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListcommentsDto.CreatePaginate();
                case CensorTypes.Route:
                    var routes = await _context.Routes.Where(route => route.VerifiedStatus == Status.Pending && route.Sections.Count > 0).Include(inc => inc.Account).Include(inc => inc.Sections).AsNoTracking().ToListAsync();
                    var routesDto = _mapper.Map<List<RouteCensorDTO>>(routes);
                    var pagingListRouteDto = PagingList<RouteCensorDTO>.OnCreate(routesDto, pagination.CurrentPage, pagination.PageSize);
                    return pagingListRouteDto.CreatePaginate();
                default:
                    return null;
            }
        }
    }

}