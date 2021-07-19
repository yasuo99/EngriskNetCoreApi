using Application.Hubs;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Persistence;

namespace Application.Services.Core
{
    public class ServiceWrapper : IServiceWrapper
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly UserManager<Account> _userManager;
        private IAccountService _account;
        private IExamOnlineScheduleService _examOnline;
        
        //hub
        private IHubContext<ExamHub> _examHub;
        public ServiceWrapper(ApplicationDbContext context, IFileService fileService, IMapper mapper, IHubContext<ExamHub> examHub, UserManager<Account> userManager)
        {
            _context = context;
            _fileService = fileService;
            _mapper = mapper;
            _examHub = examHub;
            _userManager = userManager;
        }
        public IExamOnlineScheduleService ExamOnline{
            get{
                return _examOnline ??= new ExamOnlineScheduleService(_context, _examHub,_mapper);
            }
        }
    }
}