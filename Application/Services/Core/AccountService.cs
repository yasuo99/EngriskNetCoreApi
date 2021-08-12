using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Account.Follow;
using Application.DTOs.Account.Sharing;
using Application.DTOs.Certificate;
using Application.DTOs.Exam;
using Application.DTOs.Group;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.DTOs.Ticket.VerifyTicket;
using Application.DTOs.Word;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _repo;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly UserManager<Account> _userManager;
        private readonly IExamService _examService;
        private readonly INotificationService _notificationService;
        private readonly IWordService _wordService;
        private readonly IQuizService _quizService;
        private readonly ICertificateService _certificateService;
        private readonly IRouteService _routeService;
        private AccountExam _accountExam;
        private AccountQuiz _accountQuiz;
        private BoxChat _boxChat;
        public AccountService(IFileService fileService, IMapper mapper, ApplicationDbContext repo, UserManager<Account> userManager, IExamService examService, INotificationService notificationService, IWordService wordService, IQuizService quizService, ICertificateService certificateService, IRouteService routeService)
        {
            _fileService = fileService;
            _mapper = mapper;
            _repo = repo;
            _userManager = userManager;
            _examService = examService;
            _notificationService = notificationService;
            _wordService = wordService;
            _quizService = quizService;
            _certificateService = certificateService;
            _routeService = routeService;
        }
        /// <summary>
        /// Tạo box chat hỗ trợ trao đổi giữa giảng viên/giáo viên với các thành viên
        /// </summary>
        /// <param name="boxChat">Object có kiểu là BoxChat</param>
        /// <returns>BoxChat</returns>
        public async Task<BoxChat> CreateBoxChatAsync(int accountId, BoxchatCreateDTO boxchatCreateDTO)
        {
            var account = await _repo.Accounts.Where(acc => acc.Id == accountId).Include(inc => inc.BoxChats).FirstOrDefaultAsync();
            var boxChat = _mapper.Map<BoxChat>(boxchatCreateDTO);
            account.BoxChats.Add(boxChat);
            if (await _repo.SaveChangesAsync() > 0)
            {
                return boxChat;
            }
            return null;
        }
        private int GetKey()
        {
            return new Random().Next(10000, 99999);
        }
        /// <summary>
        /// Tạo ticket để quản trị viên duyệt quyền là teacher/student
        /// </summary>
        /// <param name="createVerifyTicketDTO"></param>
        /// <returns></returns>

        public async Task<bool> FollowingUserAsync(Account follower, Account following)
        {
            var accountFollowing = await _repo.Accounts.Where(account => account.Id == follower.Id).Include(acc => acc.Following).FirstOrDefaultAsync();
            var followStatus = accountFollowing.Following.Where(fl => fl.FollowingId == following.Id).FirstOrDefault();
            if (followStatus != null)
            {
                accountFollowing.Following.Remove(followStatus);
                if (await _repo.SaveChangesAsync() > 0)
                {
                    var responseAccount = _mapper.Map<AccountBlogDTO>(follower);
                    await _notificationService.SendSignalRResponseToClient("UnFollow", following.Id, responseAccount);
                    return true;
                }
            }
            else
            {
                var follow = new Follow
                {
                    Follower = accountFollowing,
                    Following = following,
                    Timestamp = DateTime.Now
                };
                accountFollowing.Following.Add(follow);
                var receiver = HubHelper.NotificationClientsConnections.FirstOrDefault(ac => ac.AccountId == following.Id);

                var notification = new Notification
                {
                    Content = $"{accountFollowing.UserName} đã theo dõi bạn",
                    Type = NotificationType.info,
                    From = accountFollowing,
                    Url = $"/blog?id={accountFollowing.Id}"
                };
                var reFollow = await _repo.Notifications.FirstOrDefaultAsync(notify => notify.CreatedBy.Equals(follower.UserName) && notify.Content.Equals(notification.Content));
                if (reFollow == null)
                {
                    accountFollowing.CreatedNotification.Add(notification);
                    following.ReceivedNotification.Add(new AccountNotification
                    {
                        Notification = notification,
                        Status = NotificationStatus.Unseen
                    });
                }
                else
                {
                    reFollow.CreatedDate = DateTime.Now;
                    var receiverNotification = await _repo.AccountNotifications.FirstOrDefaultAsync(an => an.NotificationId == reFollow.Id);
                    receiverNotification.Status = NotificationStatus.Unseen;
                    notification = reFollow;
                }
                if (await _repo.SaveChangesAsync() > 0)
                {
                    var responseNotification = _mapper.Map<ResponseNotificationDTO>(notification);
                    responseNotification.Status = NotificationStatus.Unseen.ToString();
                    await _notificationService.SendSignalRResponseToClient("NewFollower", following.Id, responseNotification);
                    return true;
                }
            }

            return false;
        }

        public async Task<Account> GetAccountAsync(int id)
        {
            return await _repo.Accounts.Where(acc => acc.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetAccountRolesAsync(int id)
        {
            var account = await _repo.Accounts.Where(acc => acc.Id == id).Include(acc => acc.Roles).ThenInclude(role => role.Role).FirstOrDefaultAsync();
            var accountDto = _mapper.Map<AccountDetailDTO>(account);
            return accountDto.Roles.ToList();
        }

        public async Task<AccountDetailDTO> GetAccountWithRolesAsync(int id)
        {
            var account = await _repo.Accounts.Where(acc => acc.Id == id).Include(acc => acc.Roles).ThenInclude(role => role.Role).FirstOrDefaultAsync();
            var accountDto = _mapper.Map<AccountDetailDTO>(account);
            return accountDto;
        }
        public async Task InviteUserToBoxchat(int userId, Guid boxchatId)
        {
            var boxChat = await _repo.BoxChats.FirstOrDefaultAsync(bc => bc.Id == boxchatId);
            var sender = await GetAccountAsync(boxChat.AccountId);
            var receiver = await GetAccountAsync(userId);
            var boxChatMember = new BoxChatMember
            {
                AccountId = userId,
                BoxChatId = boxchatId,
                Status = Status.Pending
            };
            boxChat.Members.Add(boxChatMember);
            await _notificationService.SendNotification(sender, receiver, $"You have invited to groupchat {boxChat.Title}", NotificationType.yesno, "");
        }

        public Task RemoveUserFromBoxchat(int userId, Guid boxchatId)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowingDTO>> GetAccountFollowing(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<FollowingDTO>> GetAccountFollower(int id)
        {
            throw new NotImplementedException();
        }

        public async Task AcceptBoxChatInvite(int accountId, Guid notificationId, string action)
        {
            var invite = await _repo.BoxChatMembers.FirstOrDefaultAsync(inv => inv.NotificationId == notificationId);
            var notification = await _repo.AccountNotifications.FirstOrDefaultAsync(acc => acc.AccountId == accountId && acc.NotificationId == notificationId);
            _boxChat ??= await _repo.BoxChats.Where(boxchat => boxchat.Id == invite.BoxChatId).Include(inc => inc.Members).FirstOrDefaultAsync();
            var request = _boxChat.Members.FirstOrDefault(rq => rq.AccountId == accountId);
            switch (action)
            {
                case "accept":
                    request.Status = Status.Approved;
                    var host = HubHelper.NotificationClientsConnections.Where(nc => nc.AccountId == _boxChat.AccountId).ToList();
                    if (host.Count > 0)
                    {
                        await _notificationService.SendSignalRResponseToClient("ResponseInvite", _boxChat.AccountId, new { accountId = accountId, boxchatId = _boxChat.Id, status = Enum.GetName(typeof(Status), Status.Approved) });
                    }
                    break;
                case "refuse":
                    request.Status = Status.Declined;
                    var bcHost = HubHelper.NotificationClientsConnections.Where(nc => nc.AccountId == _boxChat.AccountId).ToList();
                    if (bcHost.Count > 0)
                    {
                        await _notificationService.SendSignalRResponseToClient("ResponseInvite", _boxChat.AccountId, new { accountId = accountId, boxchatId = _boxChat.Id, status = Enum.GetName(typeof(Status), Status.Declined) });
                    }
                    break;
                default: break;
            }
            notification.Status = NotificationStatus.Seen;
            await _repo.SaveChangesAsync();
        }

        public async Task<Section> CreateSectionAsync(int accountId, Section section)
        {
            var sectionsOfAccount = await _repo.AccountSections.Where(acc => acc.AccountId == accountId).ToListAsync();
            var accountSection = new AccountSection
            {
                AccountId = accountId,
                SectionId = section.Id,
            };
            _repo.AccountSections.Add(accountSection);
            await _repo.SaveChangesAsync();
            return section;
        }
        public async Task<List<GroupDTO>> GetAllUserGroupAsync(int accountId)
        {
            var groups = await _repo.Groups.Where(group => group.AccountId == accountId).ToListAsync();
            return _mapper.Map<List<GroupDTO>>(groups);
        }

        public async Task<Group> GetGroupAsync(Guid groupId)
        {
            var group = await _repo.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            return _mapper.Map<Group>(group);
        }

        public async Task WordGroupActionAsync(Group group, Word word)
        {
            var wordGroup = await _repo.WordGroups.FirstOrDefaultAsync(wg => wg.GroupId == group.Id && wg.WordId == word.Id);
            if (wordGroup != null)
            {
                _repo.WordGroups.Remove(wordGroup);
            }
            else
            {
                group.Words.Add(new WordGroup
                {
                    Word = word
                });
            }
            await _repo.SaveChangesAsync();
        }

        public async Task<WordLearntResultDTO> GetWordLearntPathAsync(int accountId)
        {
            var wordLearnResult = new WordLearntResultDTO();
            var learntWords = await _repo.WordLearnts.Where(wl => wl.AccountId == accountId).Include(inc => inc.Word).ToListAsync();
            var practiceResult = learntWords.GroupBy(groupBy => groupBy.WordId).ToDictionary(k => k.Key, lw => new { AverageAnswerTime = lw.Average(avg => avg.AnswerTime), TotalPractice = lw.Count() });
            //Các từ vựng học chắc
            var strongLearned = practiceResult.Where(q => q.Value.TotalPractice >= 7 && q.Value.AverageAnswerTime <= 5).Select(sel => sel.Key);
            wordLearnResult.StrongLearnedWordsCount = strongLearned.Count();
            wordLearnResult.StrongLearnedWords = _mapper.Map<List<VocabularyLearnedDTO>>(learntWords.Where(w => strongLearned.Contains(w.WordId)).Select(sel => sel.Word).Distinct().ToList());
            foreach (var strong in strongLearned)
            {
                practiceResult.Remove(strong);
            }
            //Các từ vựng khá vững
            var mediumLearned = practiceResult.Where(q => q.Value.TotalPractice >= 5 && q.Value.AverageAnswerTime <= 10).Select(sel => sel.Key);
            wordLearnResult.MediumLearnedWordsCount = mediumLearned.Count();
            wordLearnResult.MediumLearnedWords = _mapper.Map<List<VocabularyLearnedDTO>>(learntWords.Where(w => mediumLearned.Contains(w.WordId)).Select(sel => sel.Word).ToList().Distinct());
            foreach (var medium in mediumLearned)
            {
                practiceResult.Remove(medium);
            }
            //Các từ vựng học yếu
            var weakLearned = practiceResult.Select(sel => sel.Key);
            wordLearnResult.WeakLearnedWordsCount = weakLearned.Count();
            wordLearnResult.WeakLearnedWords = _mapper.Map<List<VocabularyLearnedDTO>>(learntWords.Where(w => weakLearned.Contains(w.WordId)).Select(sel => sel.Word).ToList().Distinct());
            foreach (var weak in weakLearned)
            {
                practiceResult.Remove(weak);
            }
            //Set ranking cho từ vựng
            wordLearnResult.WeakLearnedWords = SetMasterRanking(wordLearnResult.WeakLearnedWords, "weak");
            wordLearnResult.MediumLearnedWords = SetMasterRanking(wordLearnResult.MediumLearnedWords, "medium");
            wordLearnResult.StrongLearnedWords = SetMasterRanking(wordLearnResult.StrongLearnedWords, "strong");
            return wordLearnResult;
        }

        public async Task SwitchNotificationAsync(Account follower, Account following)
        {
            var accountFollow = await _repo.Follows.Where(f => f.FollowerId == follower.Id && f.FollowingId == following.Id).FirstOrDefaultAsync();
            accountFollow.NotificationSwitch = accountFollow.NotificationSwitch ? false : true;
            await _repo.SaveChangesAsync();
        }
        private List<VocabularyLearnedDTO> SetMasterRanking(List<VocabularyLearnedDTO> source, string ranking)
        {
            foreach (var item in source)
            {
                item.LearnStatus = ranking.ToLower();
            }
            return source;
        }

        public async Task<List<QuestionDTO>> VocabularyReviewAsync(int accountId, string option)
        {
            return await _wordService.WordLearnedReviewAsync(accountId, option);
        }

        public async Task<bool> CheckReviewQuestionAnswer(int accountId, Guid questionId, Guid answerId)
        {
            return await _wordService.CheckReviewQuestionAsync(questionId, answerId);
        }

        public async Task<dynamic> GetQuizSharingInformationAsync(int accountId, Guid quizId)
        {
            var accountSharings = await _repo.AccountShares.Where(ac => ac.OwnerId == accountId).Include(inc => inc.Quiz).Include(inc => inc.ShareTo).Where(q => q.QuizId == quizId).OrderBy(orderBy => orderBy.CreatedDate).ToListAsync();
            //Group dữ liệu dựa trên quizId
            var quizGrouped = accountSharings.GroupBy(groupBy => new { groupBy.QuizId, groupBy.Quiz.QuizName }).ToDictionary(k => k.Key, val => val.Select(sel => new { id = sel.ShareToId, username = sel.ShareTo.UserName }));
            return new
            {
                Sharing = quizGrouped.Values.FirstOrDefault(),
                Users = await _repo.Accounts.Where(acc => acc.Roles.Any(r => r.Role.Name == "learner") && acc.Id != accountId).Select(sel => new { username = sel.UserName, id = sel.Id }).ToListAsync()
            };
        }

        public async Task<bool> SharingQuizAsync(int accountId, Guid quizId, List<int> users)
        {
            return await _quizService.ShareQuizAsync(quizId, accountId, users);
        }

        public async Task<int> CheckAccounts(List<int> users)
        {
            foreach (var id in users)
            {
                if (!await _repo.Accounts.AnyAsync(acc => acc.Id == id))
                {
                    return id;
                }
            }
            return -1;
        }

        public async Task<dynamic> GetExamSharingInformationAsync(int accountId, Guid examId)
        {
            var accountSharings = await _repo.AccountShares.Where(ac => ac.OwnerId == accountId).Include(inc => inc.Quiz).Include(inc => inc.Exam).Include(inc => inc.ShareTo).Where(q => q.ExamId == examId).OrderBy(orderBy => orderBy.CreatedDate).ToListAsync();
            //Group dữ liệu dựa trên quizId
            var examGrouped = accountSharings.GroupBy(groupBy => new { groupBy.ExamId, groupBy.Exam.Title }).ToDictionary(k => k.Key, val => val.Select(sel => new { id = sel.ShareToId, username = sel.ShareTo.UserName }));
            return new
            {
                Sharing = examGrouped.Values.FirstOrDefault(),
                Users = await _repo.Accounts.Where(acc => acc.Roles.Any(r => r.Role.Name == "learner") && acc.Id != accountId).Select(sel => new { username = sel.UserName, id = sel.Id }).ToListAsync()
            };
        }

        public async Task<bool> SharingExamAsync(int accountId, Guid examId, List<int> users)
        {
            return await _examService.ShareExamAsync(accountId, examId, users);
        }
        public async Task<PaginateDTO<QuizDTO>> GetSharedQuizAsync(int accountId, PaginationDTO pagination)
        {
            var sharedQuiz = await _repo.AccountShares.Where(share => share.ShareToId == accountId && share.QuizId != null).Include(inc => inc.Quiz).ThenInclude(inc => inc.Questions).Select(sel => sel.Quiz).ToListAsync();
            var quizDTO = _mapper.Map<List<QuizDTO>>(sharedQuiz);
            var pagingQuizzes = PagingList<QuizDTO>.OnCreate(quizDTO, pagination.CurrentPage, pagination.PageSize);
            return pagingQuizzes.CreatePaginate();
        }
        public async Task<PaginateDTO<ExamDTO>> GetSharedExamAsync(int accountId, PaginationDTO pagination)
        {
            var sharedExam = await _repo.AccountShares.Where(share => share.ShareToId == accountId && share.ExamId != null).Include(inc => inc.Exam).ThenInclude(inc => inc.Questions).Select(sel => sel.Exam).ToListAsync();
            var examDTO = _mapper.Map<List<ExamDTO>>(sharedExam);
            var pagingListExams = PagingList<ExamDTO>.OnCreate(examDTO, pagination.CurrentPage, pagination.PageSize);
            return pagingListExams.CreatePaginate();
        }

        public async Task<bool> DeleteQuizAsync(int accountId, Guid quizId)
        {
            return await _quizService.DeleteQuizAsync(quizId);
        }

        public async Task<bool> DeleteExamAsync(int accountId, Guid examId)
        {
            return await _examService.DeleteExamAsync(examId);
        }

        public async Task<bool> CheckQuizAsync(int accountId, Guid quizId)
        {
            return await _repo.AccountQuizzes.AnyAsync(aq => aq.AccountId == accountId && aq.QuizId == quizId);
        }

        public async Task<bool> CheckExamAsync(int accountId, Guid examId)
        {
            return await _repo.AccountExams.AnyAsync(aq => aq.AccountId == accountId && aq.ExamId == examId);
        }

        public async Task<List<BoxchatDTO>> GetUserBoxchatAsync(int accountId)
        {
            var userBoxchats = await _repo.BoxChats.Where(bc => bc.AccountId == accountId || bc.Members.Any(mem => mem.AccountId == accountId && mem.Status == Status.Approved)).Include(inc => inc.Account).Include(inc => inc.Members).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking().ToListAsync();
            var boxchatDTO = _mapper.Map<List<BoxchatDTO>>(userBoxchats);
            return boxchatDTO;
        }

        public async Task<BoxchatDTO> GetBoxchatMessageAsync(int accountId, Guid boxChatId)
        {
            var boxchat = await _repo.BoxChats.Where(bc => bc.Id == boxChatId).Include(inc => inc.Account).Include(inc => inc.Members).ThenInclude(inc => inc.Account).FirstOrDefaultAsync();
            if (boxchat.AccountId != accountId && !boxchat.Members.Any(mem => mem.AccountId == accountId))
            {
                return null;
            }
            boxchat.Messages = await _repo.Messages.Where(ms => ms.BoxChatId == boxchat.Id).Include(inc => inc.Account).OrderBy(orderBy => orderBy.CreatedDate).ToListAsync();
            var boxchatDTO = _mapper.Map<BoxchatDTO>(boxchat);
            return boxchatDTO;
        }

        public async Task InviteUsersToBoxchatAsync(Guid boxchatId, List<int> users)
        {
            _boxChat ??= await _repo.BoxChats.Where(bc => bc.Id == boxchatId).Include(inc => inc.Members).Include(inc => inc.Account).FirstOrDefaultAsync();
            var removeUsers = _boxChat.Members.Where(mem => !users.Any(id => id == mem.AccountId)).Select(sel => sel.AccountId).ToList();
            var newUsers = users.Where(id => !_boxChat.Members.Any(mem => mem.AccountId == id)).ToList();
            foreach (var id in removeUsers)
            {
                var member = await _repo.BoxChatMembers.FirstOrDefaultAsync(bcMem => bcMem.AccountId == id && bcMem.BoxChatId == boxchatId);
                _repo.Remove(member);
            }
            var notification = new Notification
            {
                Content = $"{_boxChat.Account.UserName} đã mời bạn vào nhóm chat",
                Type = NotificationType.yesno,
                FromId = _boxChat.AccountId
            };
            foreach (var user in newUsers)
            {
                _repo.Notifications.Add(notification);
                _boxChat.Members.Add(new BoxChatMember
                {
                    AccountId = user,
                    Status = Status.Pending,
                    Notification = notification
                });
                var accountNotification = new AccountNotification
                {
                    Notification = notification,
                    Status = NotificationStatus.Unseen,
                    AccountId = user
                };
                _repo.Add(accountNotification);
            }
            if (await _repo.SaveChangesAsync() > 0)
            {
                var returnNotification = _mapper.Map<ResponseNotificationDTO>(notification);
                await _notificationService.SendSignalRResponseToClients("NewNotification", newUsers, returnNotification);
            }
        }

        public async Task<bool> CheckBoxchatOwnerAsync(Guid boxchatId, int id)
        {
            _boxChat ??= await _repo.BoxChats.Where(bc => bc.Id == boxchatId).Include(inc => inc.Members).Include(inc => inc.Account).FirstOrDefaultAsync();
            if (_boxChat == null || _boxChat.AccountId != id)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CheckBoxchatInviteAsync(Guid notificationId, int id)
        {
            var invite = await _repo.BoxChatMembers.FirstOrDefaultAsync(inv => inv.NotificationId == notificationId && inv.Status == Status.Pending);
            _boxChat ??= await _repo.BoxChats.Where(bc => bc.Id == invite.BoxChatId).Include(inc => inc.Members).Include(inc => inc.Account).FirstOrDefaultAsync();
            return invite != null;
        }

        public async Task<ResourcesSharedDTO> GetSharedResourcesWithUserAsync(int accountId)
        {
            var resourcesShared = new ResourcesSharedDTO
            {
                Exams = _mapper.Map<List<ExamDTO>>(await _examService.GetSharedExamAsync(accountId)),
                Quizzes = _mapper.Map<List<QuizDTO>>(await _quizService.GetSharedQuizAsync(accountId))
            };
            return resourcesShared;
        }

        public async Task<Word> GetWordAsync(Guid id)
        {
            return await _wordService.GetDetailAsync(id);
        }

        public async Task<DailyLearningDTO> GetDailyLearningAsync(int accountId, DateRangeDTO dateRange)
        {
            var dailyLearning = new DailyLearningDTO();
            var dayStudies = await _repo.DayStudies.Where(ds => ds.AccountId == accountId).OrderBy(orderBy => orderBy.Date).AsNoTracking().ToListAsync();
            var range = dateRange.End.Date.Subtract(dateRange.Start).Days;
            for (int i = 0; i <= range; i++)
            {
                var day = dateRange.Start.AddDays(i);
                var studiedResult = dayStudies.FirstOrDefault(ds => ds.Date.Equals(day));
                if (studiedResult == null)
                {
                    var dayStudy = new DayStudy
                    {
                        Date = day.Date,
                        AccountId = accountId
                    };
                    dailyLearning.DayStudied.Add(day, new LearningResultDTO
                    {
                        Status = false,
                        LearnedVocabulary = 0,
                        SectionDone = 0
                    });
                }
                else
                {
                    dailyLearning.DayStudied.Add(day, new LearningResultDTO
                    {
                        Status = true,
                        LearnedVocabulary = studiedResult.TotalWords,
                        SectionDone = studiedResult.TotalSections,
                        ListeningScript = studiedResult.TotalListening,
                        ReadingScript = studiedResult.TotalReading,
                        ConversationScript = studiedResult.TotalConversation,
                        WritingScript = studiedResult.TotalWriting,
                        GrammarScript = studiedResult.TotalGrammar,
                        VocabularyScript = studiedResult.TotalVocabulary,
                    });
                }
            }
            return dailyLearning;
        }

        public async Task<PaginateDTO<AccountCertificate>> GetUserCertificatesAsync(PaginationDTO pagination, int accountId, string search)
        {
            return await _certificateService.GetUserCertificatesAsync(pagination, accountId, search);
        }

        public async Task<PaginateDTO<QuestionDTO>> GetUserQuestionAsync(PaginationDTO pagination, QuestionType type, int accountId, string search = null)
        {
            var questions = await _repo.Questions.Where(q => q.AccountId == accountId).Include(inc => inc.Answers).Include(inc => inc.Exams).Include(inc => inc.Scripts).Include(inc => inc.Words).Include(inc => inc.Quizes).ToListAsync();
            if (search != null)
            {
                questions = questions.Where(q => (!string.IsNullOrEmpty(q.PreQuestion) && q.PreQuestion.ToLower().Contains(search.ToLower()) || (!string.IsNullOrEmpty(q.Content) && q.Content.ToLower().Contains(search.ToLower())))).ToList();
            }
            if (type != QuestionType.None)
            {
                questions = questions.Where(question => question.Type == type).ToList();
            }
            var questionsDto = _mapper.Map<List<QuestionDTO>>(questions);
            var pagingListQuestionsDto = PagingList<QuestionDTO>.OnCreate(questionsDto, pagination.CurrentPage, pagination.PageSize);
            return pagingListQuestionsDto.CreatePaginate();
        }

        public async Task<bool> SelectRouteAsync(int accountId, Guid routeId)
        {
            return await _routeService.SelectRouteAsync(routeId, accountId);
        }

        public async Task<bool> DeleteBoxchatAsync(Guid boxchatId)
        {
            _boxChat ??= await _repo.BoxChats.Include(inc => inc.Members).FirstOrDefaultAsync(bc => bc.Id == boxchatId);
            if(_boxChat.Members.Count > 0){
                await _notificationService.SendSignalRResponseToClients("DeleteBoxChat",_boxChat.Members.Select(sel => sel.AccountId).ToList(), "");
            }
            _repo.Remove(_boxChat);
            if (await _repo.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateBoxchatAsync(Guid boxchatId, BoxchatUpdateDTO boxchatUpdate)
        {
            var boxchat = await _repo.BoxChats.FirstOrDefaultAsync(bc => bc.Id == boxchatId);
            _mapper.Map(boxchatUpdate, boxchat);
            if (await _repo.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> MakeUserFinishRouteAsync(int accountId, Guid routeId)
        {
            var route = await _repo.Routes.Where(route => route.Id == routeId).Include(inc => inc.Sections).ThenInclude(inc => inc.Scripts).FirstOrDefaultAsync();
            foreach(var section in route.Sections){
                var sp = await _repo.SectionProgresses.Where(sp => sp.SectionId == section.Id && sp.AccountId == accountId).FirstOrDefaultAsync();
                if(sp == null){
                    sp = new SectionProgress{
                        SectionId = section.Id,
                        AccountId = accountId,
                        IsDone = true
                    };
                    _repo.SectionProgresses.Add(sp);
                    await _repo.SaveChangesAsync();
                }else{
                    sp.IsDone = true;
                }
                foreach(var script in section.Scripts){
                    var sdp = await _repo.SectionDetailProgresses.Where(sdp => sdp.SectionProgressId == sp.Id && sdp.ScriptId == script.Id).FirstOrDefaultAsync();
                    if(sdp == null){
                        sdp = new SectionDetailProgress{
                            SectionProgress = sp,
                            IsDone = true,
                            Script = script
                        };
                        _repo.SectionDetailProgresses.Add(sdp);
                    }else{
                        sdp.IsDone = true;
                    }
                    if(script.Type == ScriptTypes.Certificate){
                        var exam = await _repo.Exam.FirstOrDefaultAsync(e => e.ScriptId == script.Id);
                        var examHistory = new ExamHistory{
                            AccountId = accountId,
                            Score = 990,
                            ReceivedCertificate = false,
                            ExamId = exam.Id
                        };
                        _repo.ExamHistories.Add(examHistory);
                    }
                }
            }
            if(await _repo.SaveChangesAsync() > 0){
                return true;
            }
            return false;
        }
    }
}