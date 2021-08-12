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
        private ICertificateService _certificateService;
        private Account _account;
        private Exam _exam;
        private AccountExam _accountExam;
        public ExamService(ApplicationDbContext context, IMapper mapper, IFileService fileService, IQuestionService questionService, INotificationService notificationService, ICertificateService certificateService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
            _questionService = questionService;
            _notificationService = notificationService;
            _certificateService = certificateService;
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
            exam.PublishStatus = PublishStatus.UnPublished;
            exam.Purpose = ExamPurposes.Test;
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
            var questions = await _context.ExamQuestions.Where(q => q.ExamId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).OrderBy(orderBy => orderBy.Question.Toeic).ToListAsync();
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
                    };
                    _context.ExamHistories.Add(examHistory);
                    await _context.SaveChangesAsync();
                }
            }
            return returnExam;
        }

        public async Task<ExamResultDTO> SubmitExamAsync(Guid id, HashSet<DTOs.Answer.AnswerDTO> answers)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateExamAsync(Guid id, ExamDTO examCreateDTO)
        {
            var exam = await _context.Exam.Where(e => e.Id == id).Include(inc => inc.Questions).FirstOrDefaultAsync();
            var newQuestions = examCreateDTO.Questions.Where(q => !exam.Questions.Any(question => question.QuestionId == q.Id)).Select(sel => sel.Id).ToList();
            var removeQuestions = exam.Questions.Where(q => !examCreateDTO.Questions.Any(question => question.Id == q.QuestionId)).Select(sel => sel.QuestionId).ToList();
            await _questionService.ChangeStatusAsync(newQuestions);
            await _questionService.ChangeStatusAsync(removeQuestions);
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

        public async Task<PaginateDTO<ExamDTO>> GetExams(PaginationDTO pagination, ExamPurposes purpose = ExamPurposes.None, DifficultLevel difficult = DifficultLevel.None, string search = null, string sort = null)
        {
            var exams = from e in _context.Exam.OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking() select e;
            if (purpose != ExamPurposes.None)
            {
                exams = exams.Where(e => e.Purpose == purpose);
            }
            if (difficult != DifficultLevel.None)
            {
                exams = exams.Where(e => e.Difficult == difficult);
            }
            if (search != null)
            {
                exams = exams.Where(e => (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(search.Trim().ToLower()) || (!string.IsNullOrEmpty(e.Detail)) && e.Detail.ToLower().Contains(search.Trim().ToLower())));
            }
            if (purpose != ExamPurposes.None)
            {
                exams = exams.Where(e => e.Purpose == purpose && e.PublishStatus == PublishStatus.Published);
            }
            if (sort == "Hot")
            {
                exams = exams.OrderByDescending(orderBy => orderBy.AccessCount);
            }
            var pagingListExams = await PagingList<Exam>.OnCreateAsync(exams, pagination.CurrentPage, pagination.PageSize);
            var result = pagingListExams.CreatePaginate();
            var examsDTO = _mapper.Map<List<ExamDTO>>(result.Items);
            foreach (var exam in examsDTO)
            {
                var temp = result.Items.Where(e => e.Id == exam.Id).FirstOrDefault();
                exam.ForScript = temp.ScriptId != null ? true : false;
                exam.Questions = _mapper.Map<List<QuestionDTO>>(await _context.ExamQuestions.Where(e => e.ExamId == exam.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync());
            }
            return new PaginateDTO<ExamDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                TotalItems = result.TotalItems,
                Items = examsDTO,
                TotalPages = result.TotalPages
            };
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
            var examHistory = await _context.ExamHistories.FirstOrDefaultAsync(eh => eh.ExamId == id && eh.AccountId == accountId && eh.IsDoing == true);
            foreach (var examQuestion in examAnswer.Questions)
            {
                var userAnswer = answers.FirstOrDefault(ans => ans.QuestionId == examQuestion.Id);
                var answer = examQuestion.Answers.FirstOrDefault(ans => ans.Answer == userAnswer.Answer);
                bool result = answer != null ? await _questionService.CheckAnswerAsync(examQuestion.Id, answer.Answer) : false;
                if (result)
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
                examQuestion.UserAnswer = answer != null ? answer.Answer : "";
                if (answer != null)
                {
                    examHistory.AccountAnswers.Add(new AccountAnswer
                    {
                        QuestionId = examQuestion.Id,
                        AnswerId = answer.Id,
                        Result = result
                    });
                }
            }
            int score = listening * 5 + reading * 5;

            examHistory.IsDone = true;
            examHistory.IsDoing = false;
            examHistory.Score = score;
            examHistory.Listening = listening;
            examHistory.Reading = reading;
            examHistory.TotalTime = (int)DateTime.Now.Subtract(examHistory.Timestamp_start).TotalMinutes;

            _exam.AccessCount += 1;
            var examResultDto = new ExamResultDTO
            {
                Score = score,
                Listening = listening,
                Reading = reading,
                Answer = examAnswer,
                Spent = examHistory.TotalTime,
                Timestamp_start = examHistory.Timestamp_start,
                Timestamp_end = examHistory.Timestamp_end,
                Purpose = Enum.GetName(typeof(ExamPurposes), _exam.Purpose)
            };
            if (_exam.ScriptId != null)
            { //Check exam is belong to script
                if (rightAnswercount >= Math.Round(_exam.Questions.Count * 0.8))
                {
                    var script = await _context.Scripts.Where(scr => scr.Id == _exam.ScriptId).Include(inc => inc.Certificate).FirstOrDefaultAsync();
                    if (script.Certificate != null)
                    {
                        examResultDto.IsAbleToGetCertificate = true;
                        examResultDto.IsCertificateExam = true;
                    };

                }
                else
                {
                    examResultDto.IsAbleToGetCertificate = false;
                }

            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return examResultDto;
            }
            else
            {
                return null;
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

        public async Task<List<ExamDTO>> GetExams(ExamPurposes purpose = ExamPurposes.None, string search = null, bool questionSort = false, bool durationSort = false)
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

        public async Task PublishAsync(Guid id, PublishStatus status)
        {
            _exam ??= await _context.Exam.FirstOrDefaultAsync(e => e.Id == id);
            _exam.PublishStatus = status;
            await _context.SaveChangesAsync();
        }

        public async Task<ExamAnalyzeDTO> GetExamAnalyzeAsync(Guid examId)
        {
            _exam = await _context.Exam.Where(e => e.Id == examId).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).FirstOrDefaultAsync();
            ExamAnalyzeDTO examAnalyzeDTO = _mapper.Map<ExamAnalyzeDTO>(_exam);
            var doneData = await _context.ExamHistories.Where(eh => eh.ExamId == examId).ToListAsync();
            examAnalyzeDTO.DonePie = doneData.GroupBy(groupBy => groupBy.IsDone).ToDictionary(data => data.Key ? "Hoàn thành" : "Chưa hoàn thành", data => data.Count());
            var gradeData = await _context.ExamHistories.Where(eh => eh.ExamId == examId && eh.IsDone).Include(inc => inc.AccountAnswers).ToListAsync();
            examAnalyzeDTO.GradePie = gradeData.GroupBy(groupBy => groupBy.Score >= _exam.PassScore).ToDictionary(key => key.Key ? "Đạt" : "Chưa đạt", data => data.Count());
            examAnalyzeDTO.ScorePie = new Dictionary<string, int>();
            foreach (var grouped in gradeData.GroupBy(group => group.Score))
            {
                if (examAnalyzeDTO.ScorePie.ContainsKey(ScoreKey(grouped.Key)))
                {
                    examAnalyzeDTO.ScorePie[ScoreKey(grouped.Key)] = examAnalyzeDTO.ScorePie[ScoreKey(grouped.Key)] + 1;
                }
                else
                {
                    examAnalyzeDTO.ScorePie[ScoreKey(grouped.Key)] = 1;
                }
            }
            foreach (var question in examAnalyzeDTO.Questions)
            {
                foreach (var answer in question.Answers)
                {
                    answer.SelectCount = gradeData.Count(count => count.AccountAnswers.FirstOrDefault(ans => ans.AnswerId == answer.Id) != null);
                }
            }
            return examAnalyzeDTO;
        }
        public string ScoreKey(int key)
        {
            if (key > 900)
            {
                return "> 900";
            };
            if (key > 800)
            {
                return "800 ~ 900";
            }
            if (key > 700)
            {
                return "700 ~ 800";
            }
            if (key > 600)
            {
                return "600 ~ 700";
            }
            if (key > 500)
            {
                return "500 ~ 600";
            }
            if (key > 400)
            {
                return "400 ~ 500";
            }
            if (key > 300)
            {
                return "300 ~ 400";
            }
            if (key > 200)
            {
                return "200 ~ 300";
            }
            if (key > 100)
            {
                return "100 ~ 200";
            }
            return "< 100";
        }

        public async Task<bool> CreateExamQuestionAsync(Guid id, QuestionCreateDTO questionCreate)
        {
            _exam = await _context.Exam.Where(e => e.Id == id).Include(inc => inc.Questions).FirstOrDefaultAsync();
            var newQuestion = await _questionService.CreateQuestionAsync(questionCreate);
            if (newQuestion != null)
            {
                _exam.Questions.Add(new ExamQuestion
                {
                    Question = newQuestion
                });
                if (await _context.SaveChangesAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public async Task<bool> GenerateQuestionAsync()
        {
            var exams = await _context.Exam.Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            var toeicQuestions = await _context.Questions.Where(q => q.Type == QuestionType.Toeic).ToListAsync();
            foreach (var exam in exams)
            {
                foreach (var question in toeicQuestions)
                {
                    if (!exam.Questions.Any(q => q.Question.Id == question.Id) && exam.Questions.Count < 200)
                    {
                        exam.Questions.Add(new ExamQuestion
                        {
                            Question = question
                        });
                        if (question.Toeic < ToeicPart.Part5)
                        {
                            exam.TotalListening += 1;
                        }
                        else
                        {
                            exam.TotalReading += 1;
                        }
                        exam.TotalScore += 5;
                        exam.PassScore += 5;
                        exam.Duration += 1;
                        question.Status = QuestionStatus.InUse;
                    }
                }
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> GenerateHistoryAsync(int accountId)
        {
            // var examHistory = await _context.ExamHistories.Where(eh => eh.AccountId == accountId).Include(inc => inc.Exam).ThenInclude(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            // foreach(var history in examHistory){
            //     var accountAnswer = await _context.AccountAnswers.Where(ac => ac.ExamHistoryId == history.Id).
            // }
            return true;
        }
    }
}