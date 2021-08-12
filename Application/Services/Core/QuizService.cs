using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence;

namespace Application.Services.Core
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IQuestionService _questionService;
        private readonly INotificationService _notificationService;
        private AccountQuiz _accountQuiz;
        public QuizService(ApplicationDbContext context, IMapper mapper, IFileService fileService, IQuestionService questionService, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
            _questionService = questionService;
            _notificationService = notificationService;
        }

        public async Task AddQuestionToQuizAsync(Guid id, Guid questionId)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id);
            quiz.Questions.Add(new QuizQuestion
            {
                Question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId)
            });
            await _context.SaveChangesAsync();
        }

        public async Task<Quiz> AdminCreateQuizAsync(QuizCreateDTO quizCreateDTO)
        {
            var quiz = _mapper.Map<Quiz>(quizCreateDTO);
            _context.Quiz.Add(quiz);
            if (await _context.SaveChangesAsync() > 0)
            {
                return quiz;
            }
            return null;
        }

        public async Task<List<QuestionDTO>> AnonymousDoQuizAsync(Guid id)
        {
           var questions = await _context.QuizQuestions.Where(q => q.QuizId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).AsNoTracking().ToListAsync();
           return _mapper.Map<List<QuestionDTO>>(questions);
        }

        public async Task<bool> CheckAccountQuizAsync(int ownerId, Guid quizId)
        {
            _accountQuiz ??= await _context.AccountQuizzes.FirstOrDefaultAsync(aq => aq.QuizId == quizId && aq.AccountId == ownerId);
            if (_accountQuiz == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CheckExistAsync(Guid id)
        {
            return await _context.Quiz.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> CheckQuestionAnswerAsync(Guid quizId, Guid questionId, Guid answerId)
        {
            var questions = await _context.QuizQuestions.Where(quiz => quiz.QuizId == quizId).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
            var question = questions.FirstOrDefault(q => q.Id == questionId);
            if (question.Answers.Any(ans => ans.Id == answerId && ans.IsQuestionAnswer))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CreateQuizQuestionAsync(Guid id, QuestionCreateDTO questionCreate)
        {
            var quiz = await _context.Quiz.Where(q => q.Id == id).Include(inc => inc.Questions).FirstOrDefaultAsync();
            var newQuestion = await _questionService.CreateQuestionAsync(questionCreate);
            if(newQuestion != null){
                quiz.Questions.Add(new QuizQuestion{
                    Question = newQuestion
                });
                if(await _context.SaveChangesAsync() > 0){
                    return true;
                }
                return false;
            }
            return false;
        }

        public async Task<bool> DeleteQuizAsync(Guid quizId)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            _context.Remove(quiz);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DoneQuizAsync(Guid id, int accountId)
        {
           var quizHistory = await _context.Histories.Where(qh => qh.QuizId == id && qh.AccountId == accountId && !qh.IsDone).FirstOrDefaultAsync();
           if(quizHistory == null){
               return false;
           }
           quizHistory.IsDone = true;
           quizHistory.EndTimestamp = DateTime.Now;
           quizHistory.TimeSpent = quizHistory.EndTimestamp.Subtract(quizHistory.StartTimestamp).Minutes;
           if(await _context.SaveChangesAsync() > 0){
               return true;
           }
           return false;
        }

        public async Task<List<QuestionDTO>> DoQuizAsync(Guid id, int accountId)
        {
            var quizHistory = await _context.Histories.Where(qh => qh.QuizId == id && qh.AccountId == accountId && !qh.IsDone).FirstOrDefaultAsync();
            if(quizHistory == null){
                quizHistory = new History{
                    QuizId = id,
                    AccountId = accountId,
                    StartTimestamp = DateTime.Now
                };
                _context.Histories.Add(quizHistory);
            }
            return await AnonymousDoQuizAsync(id);
        }

        public async Task<PaginateDTO<QuizDTO>> GetAllQuizAsync(PaginationDTO pagination,PublishStatus publishStatus = PublishStatus.None, Status status = Status.Nope, DifficultLevel difficult = DifficultLevel.None, string search = null, string sort = null)
        {
            var quizzes = from q in _context.Quiz.OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking() select q;
            if(publishStatus != PublishStatus.None){
                quizzes = quizzes.Where(q => q.PublishStatus == publishStatus);
            }
            if(difficult != DifficultLevel.None){
                quizzes = quizzes.Where(q => q.DifficultLevel == difficult);
            }
            if (search != null)
            {
                quizzes = quizzes.Where(q => (!string.IsNullOrEmpty(q.QuizName) && q.QuizName.ToLower().Contains(search.ToLower())));
            }
            if(status != Status.Nope){
                quizzes = quizzes.Where(q => q.VerifiedStatus == status);
            }
            if(sort == "Hot"){
                quizzes = quizzes.OrderByDescending(q => q.AccessCount);
            }
            
            var pagingListQuizzes = await PagingList<Quiz>.OnCreateAsync(quizzes, pagination.CurrentPage, pagination.PageSize);
            var result = pagingListQuizzes.CreatePaginate();
            var quizzesDTO = _mapper.Map<List<QuizDTO>>(result.Items);
            foreach(var quiz in quizzesDTO){
                quiz.Questions = _mapper.Map<List<QuestionDTO>>(await _context.QuizQuestions.Where(qq => qq.QuizId == quiz.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync());
            }
            return new PaginateDTO<QuizDTO>{
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Items = quizzesDTO,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        public async Task<QuizDTO> GetQuizAsync(Guid id)
        {
            var quiz = await _context.Quiz.Where(quiz => quiz.Id == id).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).FirstOrDefaultAsync();
            return _mapper.Map<QuizDTO>(quiz);
        }

        public async Task<List<QuizDTO>> GetSharedQuizAsync(int accountId)
        {
            return _mapper.Map<List<QuizDTO>>(await _context.AccountShares.Where(acs => acs.ShareToId == accountId && acs.QuizId != null).Include(inc => inc.Quiz).Select(sel => sel.Quiz).ToListAsync());
        }
        public async Task<PaginateDTO<QuizDTO>> GetUserQuizzesAsync(int accountId, PaginationDTO pagination, string search = null)
        {
            var quizzes = await _context.AccountQuizzes.Where(aq => aq.AccountId == accountId).Include(inc => inc.Quiz).ThenInclude(inc => inc.Questions).Select(sel => sel.Quiz).ToListAsync();
            if (search != null)
            {
                quizzes = quizzes.Where(q => (!string.IsNullOrEmpty(q.QuizName) && q.QuizName.ToLower().Contains(search.ToLower()))).ToList();
            }
            var quizzesDTO = _mapper.Map<List<QuizDTO>>(quizzes);
            var pagingListQuizzes = PagingList<QuizDTO>.OnCreate(quizzesDTO, pagination.CurrentPage, pagination.PageSize);
            return pagingListQuizzes.CreatePaginate();
        }

        public async Task PublishAsync(Guid id, PublishStatus status)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id);
            quiz.PublishStatus = status;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ShareQuizAsync(Guid quizId, int ownerId, string username)
        {
            _accountQuiz ??= await _context.AccountQuizzes.Where(q => q.QuizId == quizId && q.AccountId == ownerId).Include(inc => inc.Quiz).FirstOrDefaultAsync();
            var shareTo = await _context.Accounts.FirstOrDefaultAsync(acc => acc.UserName.Equals(username));
            _accountQuiz.Quiz.Shared.Add(new AccountShare
            {
                OwnerId = ownerId,
                ShareTo = shareTo
            });
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ShareQuizAsync(Guid quizId, int ownerId, List<int> users)
        {
            _accountQuiz ??= await _context.AccountQuizzes.Where(q => q.QuizId == quizId && q.AccountId == ownerId).Include(inc => inc.Quiz).ThenInclude(inc => inc.Shared).FirstOrDefaultAsync();
            var unshareList = _accountQuiz.Quiz.Shared.Where(i => !users.Any(u => u == i.ShareToId)).Select(sel => sel.ShareToId).ToList();
            //Share
            var newShare = new List<int>();
            foreach (var id in users)
            {
                if (!_accountQuiz.Quiz.Shared.Any(q => q.ShareToId == id))
                {
                    _accountQuiz.Quiz.Shared.Add(new AccountShare
                    {
                        OwnerId = ownerId,
                        ShareToId = id
                    });
                    newShare.Add(id);
                };
            }
            //Unshare
            foreach (var id in unshareList)
            {
                var shared = _accountQuiz.Quiz.Shared.FirstOrDefault(s => s.ShareToId == id);
                _context.Remove(shared);
            }

            var sender = await _context.Accounts.Where(acc => acc.Id == ownerId).FirstOrDefaultAsync();
            var notification = new Notification
            {
                Content = $"{sender.UserName} đã chia sẻ bài quiz {_accountQuiz.Quiz.QuizName} với bạn",
                Type = Domain.Enums.NotificationType.info
            };
            sender.CreatedNotification.Add(notification);
            foreach (var id in newShare)
            {
                var receiverNotification = new AccountNotification
                {
                    AccountId = id,
                    Notification = notification,
                    Status = Domain.Enums.NotificationStatus.Unseen
                };
                _context.AccountNotifications.Add(receiverNotification);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                var notificationDTO = _mapper.Map<ResponseNotificationDTO>(notification);
                await _notificationService.SendSignalRResponseToClients("NewNotification", newShare, notificationDTO);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateQuizAsync(Guid id, QuizDTO quizUpdateDto)
        {
            var quiz = await _context.Quiz.Where(quiz => quiz.Id == id).Include(inc => inc.Questions).FirstOrDefaultAsync();
            var newQuestions = quizUpdateDto.Questions.Where(q => !quiz.Questions.Any(question => question.QuestionId == q.Id)).Select(sel => sel.Id).ToList();
            var removeQuestions = quiz.Questions.Where(q => !quizUpdateDto.Questions.Any(question => question.Id == q.QuestionId)).Select(sel => sel.QuestionId).ToList();
            await _questionService.ChangeStatusAsync(newQuestions);
            await _questionService.ChangeStatusAsync(removeQuestions);
            _mapper.Map(quizUpdateDto, quiz);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> VerifyQuizAsync(Guid quizId, Status status)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            quiz.VerifiedStatus = status;
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
    }
}