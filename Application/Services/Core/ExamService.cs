using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Answer;
using Application.DTOs.Exam;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
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
    public class ExamService : IExamService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IFileService _fileService;
        private IQuestionService _questionService;
        private INotificationService _notificationService;
        private Account _account;
        private Exam _exam;
        private AccountExam _accountExam;
        public ExamService(ApplicationDbContext context, IMapper mapper, IFileService fileService, IQuestionService questionService, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
            _questionService = questionService;
            _notificationService = notificationService;
        }
        public async Task<Exam> CreateUserExamAsync(int accountId, ExamCreateDTO examCreateDTO)
        {
            examCreateDTO.ExamQuestions = JsonConvert.DeserializeObject<List<QuestionCreateDTO>>(examCreateDTO.StringifyQuestions);
            var exam = _mapper.Map<Exam>(examCreateDTO);
            foreach (var q in examCreateDTO.ExamQuestions.Select((value, i) => new { i, value }))
            {
                var question = _mapper.Map<Question>(q.value);
                if (q.value.IsAudioQuestion)
                {
                    question.AudioFileName = await _fileService.GetAudioFromWord(q.value.Content, q.value.EngVoice);
                    question.Type = QuestionType.Listening;
                }
                if (examCreateDTO.QuestionImages.Count > 0)
                {
                    if (examCreateDTO.QuestionImages[q.i] != null)
                    {
                        question.ImageFileName = _fileService.UploadFile(examCreateDTO.QuestionImages[q.i], SD.ImagePath);
                    }
                }
                exam.Questions.Add(new ExamQuestion
                {
                    Question = question
                });
            }
            _context.Exam.Add(exam);
            var accountExam = new AccountExam
            {
                ExamId = exam.Id,
                AccountId = accountId
            };
            _context.AccountExams.Add(accountExam);
            if (await _context.SaveChangesAsync() > 0)
            {
                return exam;
            }
            return null;
        }
        public async Task<Exam> CreateExamAsync(ExamDTO examCreateDTO)
        {
            var exam = _mapper.Map<Exam>(examCreateDTO);
            _context.Exam.Add(exam);
            if (await _context.SaveChangesAsync() > 0)
            {
                return exam;
            }
            return null;
        }

        public async Task<bool> DeleteExamAsync(Guid id)
        {
            var exam = await _context.Exam.FirstOrDefaultAsync(exam => exam.Id == id);
            _context.Exam.Remove(exam);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ExamDTO> DoExamAsync(int accountId, Guid id)
        {
            _exam ??= await _context.Exam.FirstOrDefaultAsync(exam => exam.Id == id);
            var returnExam = _mapper.Map<ExamDTO>(_exam);
            var questions = await _context.ExamQuestions.Where(q => q.ExamId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            returnExam.Questions = _mapper.Map<IEnumerable<QuestionDTO>>(questions);
            var checkDoing = await _context.ExamHistories.AnyAsync(eh => eh.AccountId == accountId && eh.ExamId == id && eh.IsDoing);
            if (!checkDoing)
            {
                if (questions.Count > 0)
                {
                    var examHistory = new ExamHistory
                    {
                        ExamId = id,
                        AccountId = accountId,
                        IsDoing = true,
                        Timestamp_start = DateTime.Now,
                        Timestamp_end = DateTime.Now.AddMinutes(_exam.Duration),
                        CurrentQuestion = 0,
                    };
                    _context.ExamHistories.Add(examHistory);
                    await _context.SaveChangesAsync();
                }
            }
            return returnExam;
        }

        public async Task PauseExam(Guid id, int accountId, int currentQuestion)
        {
            var examHistory = await _context.ExamHistories.FirstOrDefaultAsync(exam => exam.ExamId == id && exam.AccountId == accountId && exam.IsDoing);
            examHistory.IsDoing = false;
            examHistory.Timestamp_pause = DateTime.Now;
            examHistory.CurrentQuestion = currentQuestion;
            await _context.SaveChangesAsync();
        }
        public async Task<int> ResumeExam(Guid id, int accountId)
        {
            var examHistory = await _context.ExamHistories.FirstOrDefaultAsync(exam => exam.ExamId == id && exam.AccountId == accountId && exam.IsDoing == false && exam.Timestamp_pause != null);
            if (examHistory == null)
            {
                return -1;
            }
            examHistory.IsDoing = true;
            var exam = await _context.Exam.FirstOrDefaultAsync(e => e.Id == id);
            var remainTime = examHistory.Timestamp_end.Subtract(examHistory.Timestamp_pause);
            examHistory.TotalTime = (int)examHistory.Timestamp_pause.Subtract(examHistory.Timestamp_start).TotalMinutes;
            examHistory.Timestamp_end = DateTime.Now.AddMinutes(remainTime.TotalMinutes);
            examHistory.Timestamp_pause = DateTime.Now;
            await _context.SaveChangesAsync();
            return (int)remainTime.TotalMinutes;
        }

        public async Task<ExamResultDTO> SubmitExamAsync(Guid id, HashSet<DTOs.Answer.AnswerDTO> answers)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateExamAsync(Guid id, ExamDTO examCreateDTO)
        {
            var exam = await _context.Exam.Where(e => e.Id == id).Include(inc => inc.Questions).FirstOrDefaultAsync();
            _mapper.Map(examCreateDTO, exam);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<PaginateDTO<ExamDTO>> GetUserExamAsync(int accountId, PaginationDTO pagination, string search = null)
        {
            var exams = await _context.AccountExams.Where(ae => ae.AccountId == accountId).Include(inc => inc.Exam).ThenInclude(inc => inc.Questions).ThenInclude(inc => inc.Question).Select(sl => sl.Exam).ToListAsync();
            if (search != null)
            {
                exams = exams.Where(e => (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(search.Trim().ToLower()) || (!string.IsNullOrEmpty(e.Detail)) && e.Detail.ToLower().Contains(search.Trim().ToLower()))).ToList();
            }
            var examsDTO = _mapper.Map<List<ExamDTO>>(exams);
            var returnValue = PagingList<ExamDTO>.OnCreate(examsDTO, pagination.CurrentPage, pagination.PageSize);
            return returnValue.CreatePaginate();
        }

        public async Task<PaginateDTO<ExamDTO>> GetExams(PaginationDTO pagination, Status status = Status.Pending, string search = null)
        {
            var exams = await _context.Exam.Where(exam => exam.VerifiedStatus == status).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            if (search != null)
            {
                exams = exams.Where(e => (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(search.Trim().ToLower()) || (!string.IsNullOrEmpty(e.Detail)) && e.Detail.ToLower().Contains(search.Trim().ToLower()))).ToList();
            }
            var examsDTO = _mapper.Map<List<ExamDTO>>(exams);
            var returnValue = PagingList<ExamDTO>.OnCreate(examsDTO, pagination.CurrentPage, pagination.PageSize);
            return returnValue.CreatePaginate();
        }

        public async Task<bool> CheckExist(Guid id)
        {
            return await _context.Exam.AnyAsync(e => e.Id == id);
        }

        public async Task<ExamResultDTO> DoneExam(int accountId, Guid id, List<AnswerDTO> answers)
        {
            int listening = 0;
            int reading = 0;
            int rightAnswercount = 0;
            _exam ??= await _context.Exam.Where(e => e.Id == id).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).FirstOrDefaultAsync();
            var examAnswer = _mapper.Map<ExamAnswerDTO>(_exam);
            foreach (var examQuestion in examAnswer.Questions)
            {
                var answer = answers.FirstOrDefault(ans => ans.QuestionId == examQuestion.Id);
                if (await _questionService.CheckAnswerAsync(examQuestion.Id, answer.Id))
                {
                    if (examQuestion.Type == Enum.GetName(typeof(QuestionType), QuestionType.Listening))
                    {
                        listening += 1;
                    }
                    else { reading += 1; }
                    rightAnswercount += 1;
                    examQuestion.IsRightAnswer = true;
                }
                else
                {
                    examQuestion.IsRightAnswer = false;
                }
                examQuestion.UserAnswer = answer.Answer;
            }
            int score = listening * 5 + reading * 5;
            var examHistory = await _context.ExamHistories.FirstOrDefaultAsync(eh => eh.ExamId == id && eh.AccountId == accountId && eh.IsDoing == true);
            examHistory.IsDone = true;
            examHistory.IsDoing = false;
            examHistory.Score = score;
            if (examHistory.Timestamp_pause != DateTime.MinValue)
            {
                examHistory.TotalTime += (int)DateTime.Now.Subtract(examHistory.Timestamp_pause).TotalMinutes;
            }
            else
            {
                examHistory.TotalTime = (int)DateTime.Now.Subtract(examHistory.Timestamp_start).TotalMinutes;
            }
            _exam.AccessCount += 1;
            await _context.SaveChangesAsync();
            return new ExamResultDTO
            {
                Score = score,
                Listening = listening,
                Reading = reading,
                Answer = examAnswer
            };
        }

        public async Task<bool> CheckConditionAsync(int accountId, Guid examId)
        {
            return true;
        }

        public async Task<bool> ShareExamAsync(int accountId, Guid examId, List<int> users)
        {
            _accountExam ??= await _context.AccountExams.Where(ae => ae.AccountId == accountId && ae.ExamId == examId).Include(inc => inc.Exam).ThenInclude(inc => inc.Shared).FirstOrDefaultAsync();
            var unshareList = _accountExam.Exam.Shared.Where(s => !users.Any(u => u == s.ShareToId)).Select(sel => sel.ShareToId).ToList();
            var newShare = new List<int>();
            //share
            foreach (var id in users)
            {
                if (!_accountExam.Exam.Shared.Any(s => s.ShareToId == id))
                {
                    _accountExam.Exam.Shared.Add(new AccountShare
                    {
                        ShareToId = id,
                        OwnerId = accountId
                    });
                    newShare.Add(id);
                }
            }
            //unshare
            foreach (var id in unshareList)
            {
                var shared = _accountExam.Exam.Shared.FirstOrDefault(s => s.ShareToId == id);
                _accountExam.Exam.Shared.Remove(shared);
            }
            var sender = await _context.Accounts.Where(acc => acc.Id == accountId).FirstOrDefaultAsync();
            var notification = new Notification
            {
                Content = $"{sender.UserName} đã chia sẻ bài exam {_accountExam.Exam.Title} với bạn",
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

        public async Task<List<ExamDTO>> GetSharedExamAsync(int accountId)
        {
            return _mapper.Map<List<ExamDTO>>(await _context.AccountShares.Where(acs => acs.ShareToId == accountId && acs.ExamId != null).Include(inc => inc.Exam).Select(sel => sel.Exam).ToListAsync());
        }

        public async Task<bool> CensorContentAsync(Guid id, Status status)
        {
            var exam = await _context.Exam.FirstOrDefaultAsync(exam => exam.Id == id);
            exam.VerifiedStatus = status;
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<List<ExamDTO>> GetExams(string search = null, bool questionSort = false, bool durationSort = false)
        {
            var userExams = await _context.AccountExams.AsNoTracking().Select(sel => sel.ExamId).ToListAsync();
            var exams = await _context.Exam.Where(exam => !userExams.Any(ue => ue == exam.Id)).AsNoTracking().ToListAsync();
            var examsDto = _mapper.Map<List<ExamDTO>>(exams);
            foreach (var exam in examsDto)
            {
                exam.Questions = _mapper.Map<List<QuestionDTO>>(await _context.ExamQuestions.Where(eq => eq.ExamId == exam.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync());
            }
            if (search != null)
            {
                examsDto = examsDto.Where(e => (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(search.ToLower()) || (!string.IsNullOrEmpty(e.Detail) && e.Detail.ToLower().Contains(search.ToLower())))).ToList();
            }
            if (questionSort)
            {
                examsDto = examsDto.OrderByDescending(q => q.Questions.Count()).ToList();
            }
            if (durationSort)
            {
                examsDto = examsDto.OrderByDescending(q => q.Duration).ToList();
            }
            return examsDto;
        }

        public async Task<ExamDTO> GetExamAsync(Guid id)
        {
            var exam = await _context.Exam.Where(exam => exam.Id == id).AsNoTracking().FirstOrDefaultAsync();
            var examDto = _mapper.Map<ExamDTO>(exam);
            examDto.Questions = _mapper.Map<List<QuestionDTO>>(await _context.ExamQuestions.Where(eq => eq.ExamId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).OrderBy(orderBy => orderBy.Toeic).ToListAsync());
            return examDto;
        }
    }
}