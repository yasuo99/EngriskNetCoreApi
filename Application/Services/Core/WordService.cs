using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Application.DTOs.Example;
using Application.DTOs.Memory;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Word;
using Application.DTOs.Word.WordCategory;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Persistence;

namespace Application.Services.Core
{
    public class WordService : IWordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ITranslator _translator;
        private readonly IQuestionService _questionService;
        public WordService(IMapper mapper, IFileService fileService, ApplicationDbContext context, INotificationService notificationService, IHttpClientFactory httpFactory, IConfiguration config, ITranslator translator, IQuestionService questionService)
        {
            _mapper = mapper;
            _fileService = fileService;
            _context = context;
            _notificationService = notificationService;
            _httpFactory = httpFactory;
            _config = config;
            _translator = translator;
            _questionService = questionService;
        }

        public async Task<bool> CheckConflictAsync(WordCreateDTO wordCreateDTO)
        {
            return await _context.Word.AnyAsync(word => word.Eng.ToLower().Equals(wordCreateDTO.Eng.ToLower()) && word.Vie.Equals(wordCreateDTO.Vie));
        }

        public async Task<Example> ContributeExampleAsync(Guid wordId, Example example)
        {
            throw new NotImplementedException();
        }

        public async Task<Question> CreatePracticeQuestionAsync(Guid wordId, QuestionCreateDTO questionCreateDTO)
        {
            var question = _mapper.Map<Question>(questionCreateDTO);
            if (questionCreateDTO.IsAudioQuestion)
            {
                question.AudioFileName = await _fileService.GetAudioFromWord(questionCreateDTO.Content, questionCreateDTO.EngVoice);
            }
            return question;
        }
        private void PreProcess(Word word)
        {
            word.Eng = word.Eng.ToLower().Trim();
            word.Eng = char.ToUpper(word.Eng[0]) + word.Eng.Substring(1);
            word.Vie = word.Vie.ToLower().Trim();
            word.Vie = char.ToUpper(word.Vie[0]) + word.Vie.Substring(1);
        }
        public async Task<WordDTO> CreateWordAsync(WordCreateDTO wordCreateDTO)
        {
            var word = _mapper.Map<Word>(wordCreateDTO);
            PreProcess(word);
            word.Eng = word.Eng.FirstLetterUppercase();
            word.Vie = word.Vie.FirstLetterUppercase();
            if (wordCreateDTO.Image != null)
            {
                word.WordImg = _fileService.UploadFile(wordCreateDTO.Image, SD.ImagePath);
            }
            word.WordVoice = await _fileService.GetAudioFromWord(word.Eng, wordCreateDTO.EngVoice);
            _context.Add(word);
            await _questionService.CreateVocabularyPracticeQuestion(word);
            if (await _context.SaveChangesAsync() > 0)
            {
                var returnWord = _mapper.Map<WordDTO>(word);
                return returnWord;
            }
            return null;
        }

        public async Task<Memory> CreateWordMemoryAsync(Word word, MemoryCreateDTO memoryCreateDTO, int accountId)
        {
            var memory = _mapper.Map<Memory>(memoryCreateDTO);
            if (memoryCreateDTO.Image != null)
            {
                memory.MemImg = _fileService.UploadFile(memoryCreateDTO.Image, SD.ImagePath);
            }
            memory.AccountId = accountId;
            word.Memories.Add(memory);
            if (await _context.SaveChangesAsync() > 0)
            {
                await _notificationService.SendSignalRResponse("NewMemory", memory);
                return memory;
            }
            return null;
        }

        public Task CreateWordQuestions(Guid wordId, WordQuestionsCreateDTO wordQuestionsCreateDTO)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteMemoryAsync(Memory memory)
        {
            if (!string.IsNullOrEmpty(memory.MemImg))
            {
                _fileService.DeleteFile(memory.MemImg);
            }
            _context.Memories.Remove(memory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMemoryAsync(Guid wordId, Guid memoryId)
        {
            var word = await _context.Word.Where(w => w.Id == wordId).Include(inc => inc.Memories).FirstOrDefaultAsync();
            var memory = word.Memories.FirstOrDefault(memory => memory.Id == memoryId);
            if (!string.IsNullOrEmpty(memory.MemImg))
            {
                _fileService.DeleteFile(memory.MemImg);
            }
            word.Memories.Remove(memory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteMemoryAsync(int accountId, Guid wordId, Guid memoryId)
        {
            var word = await _context.Word.Where(w => w.Id == wordId).Include(inc => inc.Memories).FirstOrDefaultAsync();
            var memory = word.Memories.FirstOrDefault(memory => memory.Id == memoryId);
            if (memory.AccountId != accountId)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(memory.MemImg))
            {
                _fileService.DeleteFile(memory.MemImg);
            }
            word.Memories.Remove(memory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteWordAsync(Guid id)
        {
            var word = await _context.Word.Where(w => w.Id == id).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).FirstOrDefaultAsync();
            _context.Remove(word);
            _context.RemoveRange(word.Questions.Select(sel => sel.Question));
            if (await _context.SaveChangesAsync() > 0)
            {
                await _notificationService.SendSignalRResponse("DeleteWord", id);
            };
        }
        public async Task DeleteWordAsync(Word word)
        {
            _context.Remove(word);
            if (await _context.SaveChangesAsync() > 0)
            {
                await _notificationService.SendSignalRResponse("DeleteWord", word.Id);
            };
        }
        public async Task<PaginateDTO<WordDTO>> GetAll(PaginationDTO pagination)
        {
            var words = _context.Word.Include(inc => inc.Memories).Take(await _context.Word.CountAsync());
            var paginateWords = await PagingList<Word>.OnCreateAsync(words, pagination.CurrentPage, pagination.PageSize);
            var returnWords = _mapper.Map<PagingList<WordDTO>>(words);
            return returnWords.CreatePaginate();
        }

        public async Task<PaginateDTO<WordDTO>> GetAllAsync(PaginationDTO pagination, string search = null)
        {
            var words = from w in _context.Word.Where(word => word.Type == VocabularyType.Insert).OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking() select w;
            if (search != null)
            {
                words = words.Where(w => (!string.IsNullOrEmpty(w.Eng) && w.Eng.ToLower().Contains(search.ToLower())) || (!string.IsNullOrEmpty(w.Vie) && w.Vie.ToLower().Contains(search.ToLower())));
            }
            var paginateWords = await PagingList<Word>.OnCreateAsync(words, pagination.CurrentPage, pagination.PageSize);
            var result = paginateWords.CreatePaginate();
            var wordsDto = _mapper.Map<List<WordDTO>>(result.Items);
            foreach (var word in wordsDto)
            {
                word.Categories = _mapper.Map<List<WordCategoryDTO>>(await _context.Categories.Where(cate => cate.WordId == word.Id).Include(inc => inc.WordCategory).Select(sel => sel.WordCategory).AsNoTracking().ToListAsync());
            }
            return new PaginateDTO<WordDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Items = wordsDto,
                TotalPages = result.TotalPages,
                TotalItems = result.TotalItems
            };
        }

        public async Task<WordDTO> GetWordAsync(Guid id)
        {
            var word = await _context.Word.Where(word => word.Id == id).Include(inc => inc.Categories).ThenInclude(inc => inc.WordCategory).FirstOrDefaultAsync();
            return _mapper.Map<WordDTO>(word);
        }
        public async Task<Word> GetDetailAsync(Guid id)
        {
            var word = await _context.Word.FirstOrDefaultAsync(word => word.Id == id);
            return word;
        }

        public async Task<Word> GetDetailWithMemoriesAsync(Guid id)
        {
            var word = await _context.Word.Where(predicate: word => word.Id == id).Include(inc => inc.Memories).FirstOrDefaultAsync();
            return word;
        }

        public async Task<Memory> GetMemoryAsync(Word word, Guid memoryId)
        {
            return word.Memories.Where(memory => memory.Id == memoryId).FirstOrDefault();
        }

        public Task<List<WordLearntDTO>> GetWordLearneds(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<WordDTO>> SearchWordAsync(string content)
        {
            var wordFromDb = await _context.Word.Where(word => word.Eng.ToLower().Equals(content.ToLower()) || word.Vie.ToLower().Contains(content.ToLower())).ToListAsync();
            if (wordFromDb.Count == 0)
            {
                try
                {
                    var wordList = new List<Word>();
                    var url = Path.Combine(_config.GetSection("DictionaryApi:Url").Value, content);
                    var result = await _httpFactory.CreateClient().GetAsync(url);
                    result.EnsureSuccessStatusCode();
                    var response = await result.Content.ReadAsStringAsync();
                    var searchResponseDTO = JsonConvert.DeserializeObject<List<WordSearchResponseDTO>>(response);
                    foreach (var searchResponse in searchResponseDTO)
                    {
                        var word = new Word
                        {
                            Eng = searchResponse.Word,
                            Vie = await _translator.TranslateTextAsync(searchResponse.Word, "vie", "en"),
                        };
                        word.WordVoice = await _fileService.GetAudioFromWord(word.Eng, "en-US");
                        word.Spelling = searchResponse.Phonetics.FirstOrDefault().Text;
                        wordList.Add(word);
                        word.Type = VocabularyType.Search;
                        _context.Add(word);
                        await _context.SaveChangesAsync();
                    }
                    return _mapper.Map<List<WordDTO>>(wordList);
                }
                catch (HttpRequestException ex)
                {
                    return null;
                }

            }
            return _mapper.Map<List<WordDTO>>(wordFromDb);
        }
        private WordClasses MapEnumWordClasses(string partOfSpeech)
        {
            switch (partOfSpeech)
            {
                case "verb":
                    return WordClasses.Verb;
                case "noun":
                    return WordClasses.Noun;
                case "pronoun":
                    return WordClasses.Pronoun;
                case "adjective":
                    return WordClasses.Adjective;
                case "adverb":
                    return WordClasses.Adverb;
                case "preposition":
                    return WordClasses.Preposition;
                case "conjunction":
                    return WordClasses.Conjunction;
                case "interjection":
                    return WordClasses.Interjection;
                case "transitive verb":
                    return WordClasses.Transitive;
                case "intransitive verb":
                    return WordClasses.Intransitive;
                case "exclamation":
                    return WordClasses.Interjection;
                default: return WordClasses.Null;
            }
        }
        public async Task SelectMemoryAsync(int accountId, Guid wordId, Guid memoryId)
        {
            var accountCardMem = new AccountCardmem
            {
                AccountId = accountId,
                WordId = wordId,
                MemoryId = memoryId
            };
            var accountCardmems = await _context.AccountCardmems.Where(accountCardMem => accountCardMem.AccountId == accountId && accountCardMem.WordId == wordId).ToListAsync();
            _context.AccountCardmems.RemoveRange(accountCardmems);
            _context.AccountCardmems.Add(accountCardMem);
            await _context.SaveChangesAsync();
        }
        public async Task<List<WordDTO>> SuggestWord(string content)
        {
            var words = await _context.Word.Where(word => word.Eng.ToLower().Contains(content.ToLower())).Take(5).ToListAsync();
            var returnWords = _mapper.Map<List<WordDTO>>(words);
            return returnWords;
        }

        public async Task<string> TranslateAsync(string content)
        {
            return await _translator.TranslateTextAsync(content, "vi", "en");
        }

        public async Task<Word> UpdateAsync(Guid id, WordUpdateDTO wordUpdateDTO)
        {
            var word = await _context.Word.Where(w => w.Id == id).Include(inc => inc.Categories).Include(inc => inc.Examples).Include(inc => inc.Questions).FirstOrDefaultAsync();
            _mapper.Map(wordUpdateDTO, word);
            word.Eng = word.Eng.FirstLetterUppercase();
            word.Vie = word.Vie.FirstLetterUppercase();
            if (wordUpdateDTO.Image != null)
            {
                if (!string.IsNullOrEmpty(word.WordImg))
                {
                    _fileService.DeleteFile(word.WordImg);
                }
                word.WordImg = _fileService.UploadFile(wordUpdateDTO.Image, SD.ImagePath);
            }
            if (!string.IsNullOrEmpty(wordUpdateDTO.Eng) && !wordUpdateDTO.Eng.ToLower().Equals(word.Eng.ToLower()))
            {
                if (!string.IsNullOrEmpty(word.WordVoice))
                {
                    _fileService.DeleteFile(word.WordVoice);
                }

                word.WordVoice = await _fileService.GetAudioFromWord(wordUpdateDTO.Eng, wordUpdateDTO.EngVoice);
            }
            await _context.SaveChangesAsync();
            return word;
        }

        public async Task<List<QuestionDTO>> WordLearnedReviewAsync(int accountId, string option)
        {
            var practiceQuestions = new List<QuestionDTO>();
            var learntVocabulary = await _context.WordLearnts.Where(wl => wl.AccountId == accountId).ToListAsync();
            var practiceResult = learntVocabulary.GroupBy(groupBy => groupBy.WordId).ToDictionary(k => k.Key, lw => new { AverageAnswerTime = lw.Average(avg => avg.AnswerTime), TotalPractice = lw.Count() });
            switch (option)
            {
                case "all":
                    var all = practiceResult.Select(sel => sel.Key);
                    foreach (var a in all)
                    {
                        var questions = await _context.WordQuestions.Where(q => q.WordId == a).ToListAsync();
                        practiceQuestions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                    }
                    break;
                case "weak":
                    var weak = practiceResult.Where(q => q.Value.TotalPractice < 5).Select(sel => sel.Key);
                    foreach (var w in weak)
                    {
                        var questions = await _context.WordQuestions.Where(q => q.WordId == w).ToListAsync();
                        practiceQuestions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                    }
                    break;
                case "medium":
                    var medium = practiceResult.Where(q => q.Value.TotalPractice >= 5 && q.Value.AverageAnswerTime <= 10).Select(sel => sel.Key);
                    foreach (var m in medium)
                    {
                        var questions = await _context.WordQuestions.Where(q => q.WordId == m).ToListAsync();
                        practiceQuestions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                    }
                    break;
                case "strong":
                    var strong = practiceResult.Where(q => q.Value.TotalPractice >= 7 && q.Value.AverageAnswerTime <= 5).Select(sel => sel.Key);
                    foreach (var s in strong)
                    {
                        var questions = await _context.WordQuestions.Where(q => q.WordId == s).ToListAsync();
                        practiceQuestions.Add(_mapper.Map<QuestionDTO>(questions.GetOneRandomFromList()));
                    }
                    break;
                default:
                    break;
            }
            return practiceQuestions;
        }

        public async Task<bool> CheckReviewQuestionAsync(Guid questionId, Guid answerId)
        {
            return await _questionService.CheckAnswerAsync(questionId, answerId);
        }

        public async Task<List<QuestionDTO>> VocabularyReviewAsync(List<Guid> words)
        {
            var questions = new List<QuestionDTO>();
            foreach (var word in words)
            {
                var wordQuestions = await _context.WordQuestions.Where(wq => wq.WordId == word).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
                if (wordQuestions.Count > 0)
                {
                    questions.Add(_mapper.Map<QuestionDTO>(wordQuestions.GetOneRandomFromList()));
                }

            }
            return questions;
        }

        public async Task<List<WordDTO>> GetVocabularyForScriptAsync(string search = null)
        {
            var words = await _context.Word.Where(word => word.Type == VocabularyType.Insert).AsNoTracking().ToListAsync();
            if (search != null)
            {
                words = words.Where(word => (!string.IsNullOrEmpty(word.Eng) && word.Eng.ToLower().Contains(search.ToLower())) || (!string.IsNullOrEmpty(word.Vie) && word.Vie.ToLower().Contains(search.ToLower()))).ToList();
            }
            return _mapper.Map<List<WordDTO>>(words);
        }

        public async Task<List<QuestionDTO>> GetVocabularyPracticeQuestionsAsync(Guid id)
        {
            var questions = await _context.WordQuestions.Where(wq => wq.WordId == id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
            return _mapper.Map<List<QuestionDTO>>(questions);
        }

        public async Task<bool> CheckExistAsync(Guid id)
        {
            return await _context.Word.AnyAsync(word => word.Id == id);
        }

        public async Task<List<WordDTO>> GetAllAsync()
        {
            var words = await _context.Word.Where(word => word.Type == VocabularyType.Insert).Include(inc => inc.Categories).ThenInclude(inc => inc.WordCategory).AsNoTracking().ToListAsync();
            return _mapper.Map<List<WordDTO>>(words);
        }

        public async Task<bool> AddWordQuestionAsync(Guid id, List<QuestionDTO> questions)
        {
            throw new NotImplementedException();
        }

        public async Task PublishAsync(Guid id, PublishStatus status)
        {
            var word = await _context.Word.FirstOrDefaultAsync(w => w.Id == id);
            word.PublishStatus = status;
            await _context.SaveChangesAsync();
        }

        public async Task<PaginateDTO<QuestionDTO>> GetWordPracticeQuestionAsync(Guid wordId, PaginationDTO pagination, QuestionType type = QuestionType.None, string search = null)
        {
            var questions = await _context.WordQuestions.Where(wq => wq.WordId == wordId).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).ToListAsync();
            if (type != QuestionType.None)
            {
                questions = questions.Where(q => q.Type == type).ToList();
            }
            if (search != null)
            {
                questions = questions.Where(q => !string.IsNullOrEmpty(q.Content) && q.Content.Contains(search.Trim()) || !string.IsNullOrEmpty(q.PreQuestion) && q.PreQuestion.Contains(search.Trim())).ToList();
            }
            var questionsDto = _mapper.Map<List<QuestionDTO>>(questions);
            var paginlistQuestionDto = PagingList<QuestionDTO>.OnCreate(questionsDto, pagination.CurrentPage, pagination.PageSize);
            return paginlistQuestionDto.CreatePaginate();
        }

        public async Task<bool> CreateWordExample(Guid id, ExampleDTO exampleDTO)
        {
            var example = _mapper.Map<Example>(exampleDTO);
            example.VerifiedStatus = Status.Approved;
            example.Eng = example.Eng.FirstLetterUppercase();
            example.Vie = example.Vie.FirstLetterUppercase();
            var word = await _context.Word.Where(word => word.Id == id).Include(inc => inc.Examples).FirstOrDefaultAsync();
            word.Examples.Add(example);
            await _questionService.CreateVocabularyPracticeQuestion(word, example);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public Task<bool> DeleteWordExamole(Guid id, List<Guid> examples)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> GenerateWordQuestionAsync()
        {
            var words = await _context.Word.Where(w => w.Type == VocabularyType.Insert).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ToListAsync();
            foreach (var word in words)
            {

                await _questionService.FillVocabularyPracticeQuestion(word);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteFailQuestionAsync()
        {
             var words = await _context.Word.Where(w => w.Type == VocabularyType.Insert).Include(inc => inc.Questions).ThenInclude(inc => inc.Question).ThenInclude(inc => inc.Answers).ToListAsync();
            foreach (var word in words)
            {

               foreach(var q in word.Questions){
                   if(q.Question.Answers.Any(ans => string.IsNullOrEmpty(ans.Content))){
                       _context.Questions.Remove(q.Question);
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