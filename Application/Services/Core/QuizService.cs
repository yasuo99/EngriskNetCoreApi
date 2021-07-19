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
            quizCreateDTO.TempQuestions = JsonConvert.DeserializeObject<List<QuestionCreateDTO>>(quizCreateDTO.SerializeQuestions);
            var quiz = _mapper.Map<Quiz>(quizCreateDTO);
            foreach (var q in quizCreateDTO.TempQuestions.Select((value, i) => new { i, value }))
            {
                var question = _mapper.Map<Question>(q.value);
                if (q.value.IsAudioQuestion)
                {
                    question.AudioFileName = await _fileService.GetAudioFromWord(q.value.Content, q.value.EngVoice);
                }
                if (quizCreateDTO.QuestionImages.Count > 0)
                {
                    if (quizCreateDTO.QuestionImages[q.i] != null)
                    {
                        question.ImageFileName = _fileService.UploadFile(quizCreateDTO.QuestionImages[q.i], SD.ImagePath);
                    }
                }
                foreach (var a in q.value.Answers.Select((value, j) => new { j, value }))
                {
                    if (a.value.IsAudioAnswer)
                    {
                        question.Answers.ElementAt(a.j).AudioFileName = await _fileService.GetAudioFromWord(a.value.Content, SD.AudioPath);
                    }
                    if (quizCreateDTO.AnswerImages.Count > 0)
                    {
                        if (quizCreateDTO.AnswerImages[a.j] != null)
                        {
                            question.Answers.ElementAt(a.j).ImageFileName = _fileService.UploadFile(quizCreateDTO.AnswerImages[a.j], SD.ImagePath);
                        }
                    }
                }
                quiz.Questions.Add(new QuizQuestion
                {
                    Question = question
                });
            }
            _context.Quiz.Add(quiz);
            if (await _context.SaveChangesAsync() > 0)
            {
                return quiz;
            }
            return null;
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

        public async Task<Quiz> ClientCreateQuizAsync(int accountId, QuizCreateDTO quizCreateDTO)
        {
            quizCreateDTO.TempQuestions = JsonConvert.DeserializeObject<List<QuestionCreateDTO>>(quizCreateDTO.SerializeQuestions);
            var quiz = _mapper.Map<Quiz>(quizCreateDTO);
            foreach (var q in quizCreateDTO.TempQuestions.Select((value, i) => new { i, value }))
            {
                var question = _mapper.Map<Question>(q.value);
                if (q.value.IsAudioQuestion)
                {
                    question.AudioFileName = await _fileService.GetAudioFromWord(q.value.Content, q.value.EngVoice);
                }
                if (quizCreateDTO.QuestionImages.Count > 0)
                {
                    if (quizCreateDTO.QuestionImages[q.i] != null)
                    {
                        question.ImageFileName = _fileService.UploadFile(quizCreateDTO.QuestionImages[q.i], SD.ImagePath);
                    }
                }
                foreach (var a in q.value.Answers.Select((value, j) => new { j, value }))
                {
                    if (a.value.IsAudioAnswer)
                    {
                        question.Answers.ElementAt(a.j).AudioFileName = await _fileService.GetAudioFromWord(a.value.Content, SD.AudioPath);
                    }
                    if (quizCreateDTO.AnswerImages.Count > 0)
                    {
                        if (quizCreateDTO.AnswerImages[a.j] != null)
                        {
                            question.Answers.ElementAt(a.j).ImageFileName = _fileService.UploadFile(quizCreateDTO.AnswerImages[a.j], SD.ImagePath);
                        }
                    }
                }
                quiz.Questions.Add(new QuizQuestion
                {
                    Question = question
                });
            }
            _context.Quiz.Add(quiz);
            quiz.Accounts.Add(new AccountQuiz
            {
                AccountId = accountId,
                Quiz = quiz
            });
            if (await _context.SaveChangesAsync() > 0)
            {
                return quiz;
            }
            return null;
        }

        public async Task<bool> DeleteQuizAsync(Guid quizId)
        {
            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            _context.Remove(quiz);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PaginateDTO<QuizDTO>> GetAllQuizAsync(PaginationDTO pagination, Status status = Status.Approved, string search = null)
        {
            var quizzes = await _context.Quiz.Where(q => q.VerifiedStatus == status).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            if (search != null)
            {
                quizzes = quizzes.Where(q => (!string.IsNullOrEmpty(q.QuizName) && q.QuizName.ToLower().Contains(search.ToLower()))).ToList();
            }
            var quizzesDTO = _mapper.Map<List<QuizDTO>>(quizzes);
            var pagingListQuizzes = PagingList<QuizDTO>.OnCreate(quizzesDTO, pagination.CurrentPage, pagination.PageSize);
            return pagingListQuizzes.CreatePaginate();
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