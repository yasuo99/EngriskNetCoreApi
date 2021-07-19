using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.DTOs.Script;
using Application.DTOs.Section;
using Application.DTOs.Vocabulary;
using Application.DTOs.Word;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Z.EntityFramework.Plus;

namespace Application.Services.Core
{
    public class SectionService : ISectionService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IQuizService _quizService;
        private Section _anonymousSection;
        private AnonymousSettings _anonymousSettings;
        private IFileService _fileService;
        public Section AnonymousSection
        {
            get
            {
                return _anonymousSection;
            }
        }
        private Section _section;
        public SectionService(ApplicationDbContext context, IMapper mapper, IQuizService quizService, IOptions<AnonymousSettings> anonymousSettings, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _quizService = quizService;
            _anonymousSettings = new AnonymousSettings
            {
                QuizQuestionConfig = anonymousSettings.Value.QuizQuestionConfig,
                ListeningQuestionCofig = anonymousSettings.Value.ListeningQuestionCofig,
                ConversationQuestionConfig = anonymousSettings.Value.ConversationQuestionConfig
            };
            _fileService = fileService;
        }
        public async Task<bool> CheckExistAsync(Guid sectionId)
        {
            _section ??= await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            return _section != null;
        }

        public async Task<bool> CheckQuestionAnswerAsync(Guid quizId, Guid questionId, Guid answerId)
        {
            return await _quizService.CheckQuestionAnswerAsync(quizId, questionId, answerId);
        }

        public async Task DoneQuizAsync()
        {
            throw new NotImplementedException();
        }

        // public async Task<QuizDTO> DoQuizAsync(Guid sectionId, int? accountId)
        // {
        //     Quiz quiz = new Quiz();
        //     var section = await _context.Sections.Where(section => section.Id == sectionId).FirstOrDefaultAsync();
        //     //Kiểm tra xem có phải là anonymous request
        //     if (!accountId.HasValue)
        //     {
        //         quiz = section.Quizzes.FirstOrDefault();
        //         if (quiz.RequireLogin)
        //         {
        //             return null;
        //         }
        //         var quizDTO = _mapper.Map<QuizDTO>(quiz);
        //         quizDTO.Questions = _mapper.Map<List<QuestionDTO>>(await _context.QuizQuestions.Where(q => q.QuizId == quiz.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).Where(q => q.IsConversationQuestion == false).ToListAsync());
        //         return quizDTO;
        //     }
        //     var accountQuizHistories = await _context.Histories.Where(history => history.AccountId == accountId.Value).ToListAsync();
        //     if (accountQuizHistories.Any(q => q.IsDone == false))
        //     {
        //         var undoneQuiz = accountQuizHistories.FirstOrDefault(q => q.IsDone == false);
        //         var undoneQuizDTO = _mapper.Map<QuizDTO>(undoneQuiz);
        //         undoneQuizDTO.Questions = _mapper.Map<List<QuestionDTO>>(await _context.QuizQuestions.Where(q => q.QuizId == undoneQuiz.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).Where(q => q.IsConversationQuestion == false).ToListAsync());
        //         return undoneQuizDTO;
        //     }
        //     else
        //     {
        //         var sectionHistories = from q in section.Quizzes
        //                                join history in accountQuizHistories
        //                                on q.Id equals history.QuizId
        //                                select q;

        //         if (sectionHistories.Count() == 0)
        //         {
        //             quiz = section.Quizzes.FirstOrDefault();
        //         }
        //         else
        //         {
        //             //Nếu đã học thông qua quiz nào của section này, kiểm tra số quiz còn lại của section
        //             var remainQuiz = section.Quizzes.Where(quiz => sectionHistories.Contains(quiz) == false);
        //             //Nếu còn quiz chưa làm thì lấy quiz đầu tiên 
        //             if (remainQuiz.Count() > 0)
        //             {
        //                 quiz = remainQuiz.FirstOrDefault();
        //             }
        //             //Ngược lại lấy random 1 quiz trong ds quiz
        //             else
        //             {
        //                 quiz = section.Quizzes.GetOneRandomFromList();
        //             }
        //         }
        //         var quizDTO = _mapper.Map<QuizDTO>(quiz);
        //         if (quiz != null)
        //         {
        //             var questions = await _context.QuizQuestions.Where(quiz => quiz.QuizId == quiz.QuizId).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sl => sl.Question).Where(q => q.IsConversationQuestion == false).ToListAsync();
        //             quizDTO.Questions = _mapper.Map<List<QuestionDTO>>(questions);
        //             var accountHistory = new History
        //             {
        //                 AccountId = accountId.Value,
        //                 QuizId = quiz.Id,
        //                 StartTimestamp = DateTime.Now,
        //                 IsDone = false,
        //                 TimeSpent = 0
        //             };
        //             quiz.AccessCount += 1;
        //             _context.Add(accountHistory);
        //             if (await _context.SaveChangesAsync() > 0)
        //             {
        //                 return quizDTO;
        //             }
        //             return null;
        //         }
        //         return null;
        //     }
        // }

        public async Task<PaginateDTO<SectionDTO>> GetSectionsAsync(PaginationDTO pagination)
        {
            var userCreatedSection = await _context.AccountSections.Select(sel => sel.SectionId).ToListAsync();
            var sectionsFromDb = await _context.Sections.Where(sec => !userCreatedSection.Any(us => us == sec.Id)).OrderBy(orderBy => orderBy.CreatedDate).ToListAsync();
            var sectionsDTO = _mapper.Map<List<SectionDTO>>(sectionsFromDb);
            var anonymousSection = sectionsFromDb.FirstOrDefault();
            foreach (var section in sectionsDTO)
            {
                if (section.Id == anonymousSection.Id)
                {
                    section.IsCurrentLocked = false;
                }
                else
                {
                    section.IsCurrentLocked = true;
                }

                section.TotalWord = await _context.WordCategories.Where(wc => wc.SectionId == section.Id).Include(inc => inc.Words).SelectMany(sel => sel.Words).CountAsync();
            }
            var paginglistSections = PagingList<SectionDTO>.OnCreate(sectionsDTO, pagination.CurrentPage, pagination.PageSize);
            return paginglistSections.CreatePaginate();
        }

        public async Task<PaginateDTO<SectionDTO>> GetUserSectionsAndProgressAsync(PaginationDTO pagination, int accountId)
        {
            var userCreatedSection = await _context.AccountSections.Select(sel => sel.SectionId).ToListAsync();
            var sectionsFromDb = await _context.SectionProgresses.Where(sec => !userCreatedSection.Any(us => us == sec.SectionId)).Include(inc => inc.Section).ToListAsync();
            var sectionsDTO = _mapper.Map<List<SectionDTO>>(sectionsFromDb);
            var paginglistSections = PagingList<SectionDTO>.OnCreate(sectionsDTO, pagination.CurrentPage, pagination.PageSize);
            return paginglistSections.CreatePaginate();
        }

        public async Task<VocabularyLearnDTO> VocabularyLearnAsync(Guid sectionId, int accountId)
        {
            var sectionStatus = await _context.SectionProgresses.Where(sec => sec.SectionId == sectionId && sec.AccountId == accountId).Include(inc => inc.Section).FirstOrDefaultAsync();
            var previousSection = await _context.SectionProgresses.Where(sec => sec.Section.Index == sectionStatus.Section.Index - 1).FirstOrDefaultAsync();
            if (previousSection.IsDone)
            {
                sectionStatus.IsLock = false;
                var vocabularyLearn = new VocabularyLearnDTO();
                var words = await _context.WordCategories.Where(wc => wc.SectionId == sectionId).Include(inc => inc.Words).ThenInclude(inc => inc.Word).SelectMany(sel => sel.Words).ToListAsync();
                vocabularyLearn.Words = _mapper.Map<List<WordDTO>>(words.Select(sel => sel.Word).ToList());
                foreach (var word in vocabularyLearn.Words)
                {
                    if (await _context.WordLearnts.AnyAsync(wl => wl.AccountId == accountId && wl.WordId == word.Id))
                    {
                        word.IsLearned = true;
                    }
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
                    if (questions.Count > 0)
                    {
                        vocabularyLearn.Questions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                    }
                }
                vocabularyLearn.Questions = vocabularyLearn.Questions.Shuffle().ToList();
                vocabularyLearn.Words = vocabularyLearn.Words.OrderBy(orderBy => orderBy.IsLearned).Shuffle().ToList();
                return vocabularyLearn;
            }
            return null;
        }

        public async Task<SectionLearningScript> SectionLearn(Guid sectionId, int accountId, int quizQuestionConfig = 5, int listeningQuestionConfig = 5, int conversationConfig = 5)
        {

            var sectionLearningScript = new SectionLearningScript();
            var words = await _context.WordCategories.Where(wc => wc.SectionId == sectionId).Include(inc => inc.Words).ThenInclude(inc => inc.Word).SelectMany(sel => sel.Words).ToListAsync();
            var wordLearnt = await _context.WordLearnts.Where(wl => wl.AccountId == accountId).Select(sel => sel.WordId).Distinct().ToListAsync();
            var sectionWords = _mapper.Map<List<WordDTO>>(words.Select(sel => sel.Word));
            //Map từ vựng đã học và từ vựng chưa học
            sectionWords.ForEach((word) =>
            {
                if (wordLearnt.Any(wl => wl == word.Id))
                {
                    word.IsLearned = true;
                }
            });
            //Kịch bản cho phần từ vựng chưa học thông qua chức năng flashcard riêng
            var newWords = sectionWords.Where(word => !word.IsLearned).ToList();
            if (newWords.Count > 0)
            {
                var firstScript = new ScriptDTO
                {
                    Words = newWords
                };
                foreach (var word in firstScript.Words)
                {
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).Where(q => q.Type != QuestionType.Conversation).AsNoTracking().ToListAsync();
                    var selectedQuestion = questions.GetAmountRandomFromAList(2);
                    foreach (var q in selectedQuestion)
                    {
                        firstScript.Questions.Add(_mapper.Map<QuestionDTO>(q));
                    }
                }
                sectionLearningScript.Scripts.Add(firstScript);
            }
            //Kịch bản cho phần từ vựng đã học thông qua chức năng flashcard riêng
            var learnedWords = sectionWords.Where(word => word.IsLearned).ToList();
            if (learnedWords.Count > 0)
            {
                var secondScript = new ScriptDTO
                {
                    Words = learnedWords
                };
                foreach (var word in secondScript.Words)
                {
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).AsNoTracking().ToListAsync();
                    var selectedQuestion = questions.GetAmountRandomFromAList(2);
                    foreach (var q in selectedQuestion)
                    {
                        secondScript.Questions.Add(_mapper.Map<QuestionDTO>(q));
                    }
                }
                sectionLearningScript.Scripts.Add(secondScript);
            }
            return sectionLearningScript;
        }

        public async Task<bool> CheckProgressAsync(int accountId)
        {
            return await _context.SectionProgresses.AnyAsync(sp => sp.AccountId == accountId);
        }

        public async Task CreateAccountSectionProgressAsync(Guid id, int accountId)
        {
            if (!await CheckProgressAsync(accountId))
            {
                var sections = await _context.Sections.ToListAsync();
                _anonymousSection ??= sections.FirstOrDefault();
                foreach (var section in sections)
                {
                    var sectionProgress = new SectionProgress
                    {
                        SectionId = section.Id,
                        AccountId = accountId,
                    };
                    if (section.Id == _anonymousSection.Id)
                    {
                        sectionProgress.IsLock = false;
                    }
                    section.SectionProgresses.Add(sectionProgress);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckAnonymousLearnAsync(Guid id)
        {
            _anonymousSection ??= await _context.Sections.FirstOrDefaultAsync();
            return _anonymousSection.Id == id;
        }

        public async Task<SectionLearningScript> AnonymousSectionLearnAsync()
        {
            _anonymousSection ??= await _context.Sections.FirstOrDefaultAsync();
            var sectionLearningScript = new SectionLearningScript();
            var words = await _context.WordCategories.Where(wc => wc.SectionId == _anonymousSection.Id).Include(inc => inc.Words).ThenInclude(inc => inc.Word).SelectMany(sel => sel.Words).ToListAsync();
            var sectionWords = _mapper.Map<List<WordDTO>>(words.Select(sel => sel.Word));
            //Kịch bản cho phần từ vựng chưa học thông qua chức năng flashcard riêng
            var newWords = sectionWords.ToList();
            if (newWords.Count > 0)
            {
                var firstScript = new ScriptDTO
                {
                    Words = newWords
                };
                foreach (var word in firstScript.Words)
                {
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
                    firstScript.Questions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                }
                sectionLearningScript.Scripts.Add(firstScript);
            }
            return sectionLearningScript;
        }

        public async Task SectionLearnDoneAsync(Guid sectionId, int accountId)
        {
            var sectionLearning = await _context.SectionProgresses.FirstOrDefaultAsync(sp => sp.SectionId == sectionId && sp.AccountId == accountId);
            sectionLearning.IsDone = true;
            var nextSection = await _context.SectionProgresses.Where(sp => sp.SectionId != sectionId && !sp.IsDone).FirstOrDefaultAsync();
            nextSection.IsLock = false;
            await _context.SaveChangesAsync();
        }

        public async Task<SectionFinishUpDTO> SectionFinishUpAsync(Guid sectionId, int accountId, string action)
        {
            switch (action)
            {
                case "review":
                    break;
                case "finish":
                    var sectionLearning = await _context.SectionProgresses.Where(sp => sp.SectionId == sectionId && sp.AccountId == accountId).Include(inc => inc.Section).FirstOrDefaultAsync();
                    sectionLearning.IsDone = true;
                    var todayStudied = await _context.DayStudies.Where(ds => ds.Date.Date.Equals(DateTime.Now.Date) && ds.AccountId == accountId).FirstOrDefaultAsync();
                    if (todayStudied == null)
                    {
                        todayStudied ??= new DayStudy
                        {
                            Date = DateTime.Now.Date,
                            AccountId = accountId,
                            TotalSections = 1,
                        };
                        _context.Add(todayStudied);
                    }
                    else
                    {
                        todayStudied.TotalSections += 1;
                    }
                    await _context.SaveChangesAsync();
                    break;
                default:
                    break;
            }
            var dayStudies = await _context.DayStudies.Where(ds => ds.AccountId == accountId).ToListAsync();
            var nearestStudyDays = DateTime.Now.GetNearestSevenDay();
            var learningProgresses = new SectionFinishUpDTO
            {
                SectionDone = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.IsDone).CountAsync(),
                TotalSection = await _context.Sections.CountAsync(),
                LearnedVocabulary = await _context.WordLearnts.Where(wl => wl.AccountId == accountId).Select(sel => sel.WordId).Distinct().CountAsync(),
                Active_Days = dayStudies.Count,
                DayStudied = nearestStudyDays.ToDictionary(key => key.Date, data => dayStudies.Any(ds => ds.Date.Equals(data.Date)))
            };
            return learningProgresses;
        }

        public Task<PaginateDTO<SectionDTO>> GetUserPublicSectionsAndProgressAsync(PaginationDTO pagination, int ownerId, int accountId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SectionDTO>> GetFreeSectionsAsync()
        {
            var userSections = await _context.AccountSections.AsNoTracking().ToListAsync();
            var userSectionsId = userSections.Select(sel => sel.SectionId).ToList();
            var sections = await _context.Sections.Where(section => !userSectionsId.Any(id => id == section.Id) && section.RouteId == null).ToListAsync();
            return _mapper.Map<List<SectionDTO>>(sections);
        }

        public async Task<SectionLearningScript> SectionPreviewAsync(Guid id)
        {
            var sectionLearningScript = new SectionLearningScript();
            var words = await _context.WordCategories.Where(wc => wc.SectionId == id).Include(inc => inc.Words).ThenInclude(inc => inc.Word).SelectMany(sel => sel.Words).ToListAsync();
            var sectionWords = _mapper.Map<List<WordDTO>>(words.Select(sel => sel.Word));

            //Kịch bản cho phần từ vựng chưa học thông qua chức năng flashcard riêng
            var newWords = sectionWords.Where(word => !word.IsLearned).ToList();
            if (newWords.Count > 0)
            {
                var firstScript = new ScriptDTO
                {
                    Words = newWords
                };
                foreach (var word in firstScript.Words)
                {
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).Where(q => q.Type != QuestionType.Conversation).AsNoTracking().ToListAsync();
                    var selectedQuestion = questions.GetAmountRandomFromAList(2);
                    foreach (var q in selectedQuestion)
                    {
                        firstScript.Questions.Add(_mapper.Map<QuestionDTO>(q));
                    }
                }
                sectionLearningScript.Scripts.Add(firstScript);
            }
            //Kịch bản cho phần từ vựng đã học thông qua chức năng flashcard riêng
            var learnedWords = sectionWords.Where(word => word.IsLearned).ToList();
            if (learnedWords.Count > 0)
            {
                var secondScript = new ScriptDTO
                {
                    Words = learnedWords
                };
                foreach (var word in secondScript.Words)
                {
                    var questions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).AsNoTracking().ToListAsync();
                    var selectedQuestion = questions.GetAmountRandomFromAList(2);
                    foreach (var q in selectedQuestion)
                    {
                        secondScript.Questions.Add(_mapper.Map<QuestionDTO>(q));
                    }
                }
                sectionLearningScript.Scripts.Add(secondScript);
            }
            return sectionLearningScript;
        }

        public async Task<PaginateDTO<SectionDTO>> GetManageSectionsAsync(PaginationDTO pagination, string search = null)
        {
            var usersSections = await _context.AccountSections.AsNoTracking().Select(sel => sel.SectionId).ToListAsync();
            var engriskSections = await _context.Sections.Where(sec => !usersSections.Any(us => us == sec.Id)).Include(inc => inc.Route).AsNoTracking().OrderByDescending(orderBy => orderBy.CreatedDate).ToListAsync();
            if (search != null)
            {
                engriskSections = engriskSections.Where(sec => (!string.IsNullOrEmpty(sec.SectionName) && sec.SectionName.ToLower().Contains(search.ToLower())) || (!string.IsNullOrEmpty(sec.Description) && sec.Description.ToLower().Contains(search.ToLower()))).ToList();
            }
            var engriskSectionsDto = _mapper.Map<List<SectionDTO>>(engriskSections);
            var pagingListEngriskSections = PagingList<SectionDTO>.OnCreate(engriskSectionsDto, pagination.CurrentPage, pagination.PageSize);
            return pagingListEngriskSections.CreatePaginate();
        }

        public async Task<bool> DeleteSectionAsync(Guid id)
        {
            var section = await _context.Sections.FirstOrDefaultAsync(sec => sec.Id == id);
            _context.Sections.Remove(section);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CreateSectionScriptAsync(Guid sectionId, ScriptCreateDTO scriptDto)
        {
            _section ??= await _context.Sections.Where(sec => sec.Id == sectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            var script = new Script();
            if (string.IsNullOrEmpty(scriptDto.Theory))
            {
                foreach (var image in Extension.TempImagePath)
                {
                    if (scriptDto.Theory.Contains(image))
                    {
                        _fileService.DeleteFile(image);
                        Extension.TempImagePath.Remove(image);
                    }

                }
                script.Theory = scriptDto.Theory;
            }
            if (!_section.Scripts.Any(script => script.Type == scriptDto.Type))
            {
                foreach (var questionDto in scriptDto.Questions)
                {
                    var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionDto.Id);
                    if (question != null)
                    {
                        script.Questions.Add(new ScriptQuestion
                        {
                            Question = question,
                        });

                    }
                }
                foreach (var wordDto in scriptDto.Words)
                {
                    var word = await _context.Word.FirstOrDefaultAsync(w => w.Id == wordDto.Id);
                    if (word != null)
                    {
                        script.Words.Add(new ScriptWord
                        {
                            Word = word
                        });
                    }
                }
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckScriptTypeExistAsync(Guid sectionId, ScriptTypes type)
        {
            _section ??= await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            return _section.Scripts.Any(script => script.Type == type);
        }


        public async Task ScriptLearnDone(Guid id, int accountId)
        {
            var script = await _context.Scripts.FirstOrDefaultAsync(s => s.Id == id);
            switch (script.Type)
            {
                case ScriptTypes.Vocabulary:

                    break;
                case ScriptTypes.Grammar:
                    break;
                case ScriptTypes.Writing:
                    break;
                default: break;
            }
            if (script.Type == ScriptTypes.Vocabulary)
            {

            }
        }

        public async Task<SectionScriptDTO> GetSectionScriptAsync(Guid sectionId)
        {
            _section = await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).AsNoTracking().FirstOrDefaultAsync();
            foreach (var script in _section.Scripts)
            {
                script.Questions = await _context.ScriptQuestions.Where(sq => sq.ScriptId == script.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
                script.Words = await _context.ScriptWords.Where(sw => sw.ScriptId == script.Id).Include(inc => inc.Word).ToListAsync();
                script.MiniExam = await _context.Exam.Where(exam => exam.ScriptId == script.Id).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).AsNoTracking().FirstOrDefaultAsync();
            }
            return _mapper.Map<SectionScriptDTO>(_section);
        }

        public async Task<bool> CreateSectionScriptsAsync(Guid sectionId, SectionScriptCreateDTO scripts)
        {
            _section = await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            //Xử lý kịch bản cho phần bài grammar
            var grammar = scripts.Grammar;
            if (_section.Scripts.Any(script => script.Type == ScriptTypes.Grammar))
            {
                var grammarScript = _section.Scripts.FirstOrDefault(script => script.Type == ScriptTypes.Grammar);
            }
            else
            {

            }
            //Xử lý kịch bản cho phần bài từ vựng
            var vocabulary = scripts.Vocabulary;
            //Xử lý kịch bản cho phần bài từ vựng toeic
            var toeicVocabulary = scripts.ToeicVocabulary;
            //Xử lý kịch bản cho phần bài đọc
            var reading = scripts.Reading;
            //Xử lý kịch bản cho phần bài viết
            var writing = scripts.Writing;
            //Xử lý kịch bản cho phần hội thoại
            var conversation = scripts.Conversation;
            //Xử lý kịch bản cho phần bài nghe
            var listening = scripts.Listening;
            return true;
        }
        public async Task<ScriptLearnDTO> ScriptLearnAsync(Guid id, int accountId)
        {
            var script = await _context.Scripts.Where(script => script.Id == id).FirstOrDefaultAsync();
            //Kiểm tra xem đã có lưu lại tiến trình học cho section này chưa
            var sectionProgress = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.SectionId == script.SectionId).Include(inc => inc.Details).FirstOrDefaultAsync();
            if (sectionProgress == null)//Chưa lưu lại lịch sử cho section có script này
            {
                //Tạo lịch sử học cho section và script này
                sectionProgress = new SectionProgress
                {
                    AccountId = accountId,
                    SectionId = script.SectionId,
                    IsLock = false,
                };
                sectionProgress.Details.Add(new SectionDetailProgress
                {
                    ScriptId = id,
                    IsDone = false,
                });
                _context.SectionProgresses.Add(sectionProgress);
            }
            else
            {
                if (!sectionProgress.Details.Any(detail => detail.ScriptId == id))
                {
                    sectionProgress.Details.Add(new SectionDetailProgress
                    {
                        ScriptId = id,
                        IsDone = false
                    });
                }
            }
            var scriptLearnDto = _mapper.Map<ScriptLearnDTO>(script);
            var words = await _context.ScriptWords.Where(sw => sw.ScriptId == id).Include(inc => inc.Word).ThenInclude(inc => inc.Families).Include(inc => inc.Word).ThenInclude(inc => inc.Examples).ToListAsync();
            scriptLearnDto.Words = _mapper.Map<List<WordDTO>>(words);
            foreach (var word in scriptLearnDto.Words)
            {
                var wordPracticeQuestions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
                //Add vocabulary practice questions first
                scriptLearnDto.Questions.AddRange(_mapper.Map<List<QuestionDTO>>(wordPracticeQuestions.GetAmountRandomFromAList(script.VocabularySetting)));
            }

            scriptLearnDto.Questions.AddRange(_mapper.Map<List<QuestionDTO>>(await _context.ScriptQuestions.Where(sq => sq.ScriptId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync()));
            await _context.SaveChangesAsync();
            return scriptLearnDto;
        }

        public async Task<ScriptLearnDTO> ScriptDoneAsync(Guid id, int accountId)
        {
            var script = await _context.Scripts.Where(script => script.Id == id).FirstOrDefaultAsync();

            _section ??= await _context.Sections.Where(section => section.Id == script.SectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            var sectionProgress = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.SectionId == _section.Id).FirstOrDefaultAsync();
            var scriptProgress = await _context.SectionDetailProgresses.Where(sdp => sdp.SectionProgressId == sectionProgress.Id && sdp.ScriptId == id).FirstOrDefaultAsync();
            scriptProgress.IsDone = true;
            var lastDoingSection = await _context.SectionProgresses.Where(sp => sp.AccountId == accountId && sp.IsLastDoing).FirstOrDefaultAsync();
            if (lastDoingSection != null && lastDoingSection.SectionId != script.SectionId)
            {
                lastDoingSection.IsLastDoing = false;
            }
            sectionProgress.IsLastDoing = true;
            if (script.Index == _section.Scripts.Count - 1)
            {
                sectionProgress.IsDone = true;
                var nextSectionProgress = await _context.SectionProgresses.Where(section => section.Section.RouteId == _section.RouteId && section.Section.Index == _section.Index + 1).FirstOrDefaultAsync();
                if (nextSectionProgress != null)
                {
                    nextSectionProgress.IsDone = true;
                }
            }
            await _context.SaveChangesAsync();
            var nextScript = await _context.Scripts.Where(s => s.SectionId == _section.Id && s.Index == script.Index + 1).AsNoTracking().FirstOrDefaultAsync();
            if (nextScript != null)
            {
                return await ScriptLearnAsync(nextScript.Id, accountId);
            }
            return null;
        }

        public async Task<bool> CheckPreviousSectionDoneAsync(Guid id, int accountId)
        {
            var sectionProgress = await _context.SectionProgresses.Where(sp => sp.SectionId == id && sp.AccountId == accountId).Include(inc => inc.Section).FirstOrDefaultAsync();
            var previousSectionProgress = await _context.SectionProgresses.Where(sp => sp.Section.Index == sectionProgress.Section.Index - 1 && sp.AccountId == accountId).FirstOrDefaultAsync();
            if (previousSectionProgress == null)
            {
                return true;
            }
            return previousSectionProgress.IsDone;

        }

        public async Task<bool> CheckSectionScriptExistAsync(Guid sectionId, Guid scriptId)
        {
            _section = await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).FirstOrDefaultAsync();
            return _section.Scripts.Any(script => script.Id == scriptId);
        }

        public async Task<ScriptLearnDTO> AnonymousScriptLearnAsync(Guid id)
        {
            var script = await _context.Scripts.Where(script => script.Id == id).FirstOrDefaultAsync();
            var scriptLearnDto = _mapper.Map<ScriptLearnDTO>(script);
            scriptLearnDto.Words = _mapper.Map<List<WordDTO>>(await _context.ScriptWords.Where(sw => sw.ScriptId == id).Include(inc => inc.Word).ThenInclude(inc => inc.Families).ToListAsync());
            foreach (var word in scriptLearnDto.Words)
            {
                var wordPracticeQuestions = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
                scriptLearnDto.Questions.AddRange(_mapper.Map<List<QuestionDTO>>(wordPracticeQuestions.GetAmountRandomFromAList(script.VocabularySetting)));
            }
            scriptLearnDto.Questions.AddRange(_mapper.Map<List<QuestionDTO>>(await _context.ScriptQuestions.Where(sq => sq.ScriptId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync()));
            return scriptLearnDto;
        }

        public async Task<bool> CreateSectionScriptAsync(Guid sectionId, List<ScriptCreateDTO> scripts)
        {
            _section = await _context.Sections.Where(section => section.Id == sectionId).Include(inc => inc.Scripts).ThenInclude(inc => inc.Words).Include(inc => inc.Scripts).ThenInclude(inc => inc.Questions).Include(inc => inc.Scripts).ThenInclude(inc => inc.MiniExam).FirstOrDefaultAsync();
            foreach (var script in scripts)
            {
                if (Guid.Empty != script.Id && _section.Scripts.Any(s => s.Id == script.Id))
                {
                    var currentScript = _section.Scripts.FirstOrDefault(s => s.Id == script.Id);
                    if (!string.IsNullOrEmpty(script.Theory) || script.Words.Count > 0 || script.Questions.Count > 0 || script.Exam != Guid.Empty)
                    {
                        if (script.Exam != Guid.Empty)
                        {
                            if (currentScript.MiniExam.Id != script.Exam)
                            {
                                currentScript.MiniExam.ScriptId = null;
                                var exam = await _context.Exam.FirstOrDefaultAsync(e => e.Id == script.Exam);
                                exam.ScriptId = script.Id;
                            };
                        }
                        _mapper.Map(script, currentScript);
                    }
                    else
                    {
                        _section.Scripts.Remove(currentScript);
                    }
                }
                else
                {
                    var newScript = new Script();
                    if (!_section.Scripts.Any(s => s.Type == script.Type))
                    {
                        newScript = _mapper.Map<Script>(script);
                        switch (script.Type)
                        {
                            case ScriptTypes.Grammar:
                                newScript.Index = 0;
                                break;
                            case ScriptTypes.Vocabulary:
                                newScript.Index = 1;
                                break;
                            case ScriptTypes.Listening:
                                newScript.Index = 2;
                                break;
                            case ScriptTypes.Reading:
                                newScript.Index = 3;
                                break;
                            case ScriptTypes.Writing:
                                newScript.Index = 4;
                                break;
                            case ScriptTypes.Conversation:
                                newScript.Index = 5;
                                break;
                            case ScriptTypes.MiniExam:
                                newScript.Index = 6;
                                break;
                            default:
                                break;
                        }
                        if (!string.IsNullOrEmpty(newScript.Theory) || newScript.Questions.Count > 0 || newScript.Words.Count > 0 || script.Exam != Guid.Empty)
                        {
                            _section.Scripts.Add(newScript);
                            if (script.Exam != Guid.Empty)
                            {
                                var exam = await _context.Exam.Where(e => e.Id == script.Exam).Include(inc => inc.Script).FirstOrDefaultAsync();
                                exam.Script = newScript;
                            }

                        }
                    }
                }

            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
    }
}