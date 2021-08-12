using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.App;
using Application.DTOs.Quiz;
using Application.DTOs.Word;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class AppService : IAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AppService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<HomeDTO> GetHomeScreenDataAsync(int accountId)
        {
            var homeDTO = new HomeDTO();
            var userQuizzes = await _context.AccountQuizzes.Select(sel => sel.QuizId).ToListAsync();
            var sharedQuiz = await _context.AccountShares.Where(share => share.ShareToId == accountId && share.QuizId != null).Include(inc => inc.Quiz).Select(sel => sel.Quiz).ToListAsync();
            var sysQuiz = await _context.Quiz.Where(q => !userQuizzes.Any(uq => uq == q.Id)).Include(inc => inc.Questions).OrderByDescending(orderBy => orderBy.AccessCount).ToListAsync();
            homeDTO.Quizzes = _mapper.Map<List<QuizDTO>>(await _context.Quiz.OrderByDescending(orderBy => orderBy.AccessCount).Take(10).Include(inc => inc.Questions).AsNoTracking().ToListAsync());
            var topUsers = await _context.Accounts.OrderByDescending(orderBy => orderBy.UserName).Take(10).AsNoTracking().ToListAsync();
            homeDTO.Users = _mapper.Map<List<AccountBlogDTO>>(topUsers);
            var words = await _context.Word.ToListAsync();
            homeDTO.Words = _mapper.Map<List<WordDTO>>(words.GetAmountRandomFromAList(10));
            return homeDTO;
        }
    }
}