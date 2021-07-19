using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class QuestionService : IQuestionService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        public QuestionService(ApplicationDbContext context, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<bool> CheckAnswerAsync(Guid questionId, Guid answerId)
        {
            var answersOfQuestion = await _context.Questions.Where(predicate: q => q.Id == questionId).Include(inc => inc.Answers).FirstOrDefaultAsync();
            var result = answersOfQuestion.Answers.Where(ans => ans.Id == answerId).FirstOrDefault();
            return result.IsQuestionAnswer;
        }

        public async Task<bool> CheckAnswerAsync(Guid questionId, string answer)
        {

            var answersOfQuestion = await _context.Questions.Where(predicate: q => q.Id == questionId).Include(inc => inc.Answers).FirstOrDefaultAsync();
            if (answersOfQuestion.Type == QuestionType.Connection)
            {
                return CheckConnectionQuestionAnswer(answersOfQuestion, answer);
            }
            if (answersOfQuestion.Type == QuestionType.Arrange)
            {
                return CheckArrangeQuestionAnswer(answersOfQuestion, answer);
            }
            if (answersOfQuestion.Type == QuestionType.Conversation)
            {
                return CheckConversationQuestionAnswer(answersOfQuestion, answer);
            }
            var result = answersOfQuestion.Answers.Where(ans => ans.Content.ToLower().Equals(answer.ToLower())).FirstOrDefault();
            if (result == null)
            {
                return false;
            }
            return result.IsQuestionAnswer;
        }
        private bool CheckConnectionQuestionAnswer(Question question, string serializedAnswer)
        {
            var splittedAnswers = serializedAnswer.Split('_', StringSplitOptions.RemoveEmptyEntries);
            foreach (var answer in splittedAnswers)
            {
                if (!question.Answers.Any(ans => ans.Content.Equals(answer)))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckConversationQuestionAnswer(Question question, string serializedAnswer)
        {
            var splittedAnswers = serializedAnswer.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var hiddenAnswer = question.Answers.Where(ans => ans.Content.Contains("<p>")).ToList();
            hiddenAnswer = hiddenAnswer.OrderBy(orderBy => Int32.Parse(orderBy.Content.Substring(orderBy.Content.Length - 2, 1))).ToList();
            for (int i = 0; i < hiddenAnswer.Count; i++)
            {
                if (!hiddenAnswer[i].Content.Contains(splittedAnswers[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckArrangeQuestionAnswer(Question question, string answer)
        {
            if (!question.Content.Equals(answer))
            {
                return false;
            }
            return true;
        }

        public async Task VocabularyPracticeAsync(Guid questionId, int accountId)
        {
            var vocabulary = await _context.WordQuestions.Where(wq => wq.QuestionId == questionId).ToListAsync();
            foreach (var voca in vocabulary)
            {
                var wordLearnt = new WordLearnt
                {
                    WordId = voca.WordId,
                    AccountId = accountId
                };
                var dayStudy = await _context.DayStudies.FirstOrDefaultAsync(ds => ds.Date.Equals(DateTime.Now.Date) && ds.AccountId == accountId);
                if (dayStudy != null)
                {
                    dayStudy.TotalWords += 1;
                }else{
                    _context.DayStudies.Add(new Domain.Models.Version2.DayStudy{
                        Date = DateTime.Now.Date,
                        AccountId = accountId
                    });
                }

                _context.Add(wordLearnt);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PaginateDTO<QuestionDTO>> GetQuestionsAsync(PaginationDTO pagination, QuestionType type, GrammarQuestionType grammar, string search = null)
        {
            var questions = await _context.Questions.Include(inc => inc.Answers).AsNoTracking().ToListAsync();
            if(search != null){
                questions = questions.Where(q => (!string.IsNullOrEmpty(q.PreQuestion) && q.PreQuestion.ToLower().Contains(search.ToLower()) || (!string.IsNullOrEmpty(q.Content) && q.Content.ToLower().Contains(search.ToLower())))).ToList();
            }
            if(type != QuestionType.None){
                questions = questions.Where(question => question.Type == type).ToList();
            }
             if(grammar != GrammarQuestionType.None){
                questions = questions.Where(question => question.Grammar == grammar).ToList();
            }
            var questionsDto = _mapper.Map<List<QuestionDTO>>(questions);
            var paginglistQuestionDto = PagingList<QuestionDTO>.OnCreate(questionsDto,pagination.CurrentPage,pagination.PageSize);
            return paginglistQuestionDto.CreatePaginate();
        }
        
        public async Task<List<QuestionDTO>> GetQuestionsAsync(QuestionType type , GrammarQuestionType grammar)
        {
            var questions = await _context.Questions.Include(inc => inc.Answers).AsNoTracking().ToListAsync();
            if(type != QuestionType.None){
                questions = questions.Where(question => question.Type == type).ToList();
            }
            if(grammar != GrammarQuestionType.None){
                questions = questions.Where(question => question.Grammar == grammar).ToList();
            }
            return _mapper.Map<List<QuestionDTO>>(questions);
        }

        public async Task<StatisticalDTO> GetStatisticalAsync()
        {
            var questionUsingForQuiz = await _context.QuizQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var questionUsingForExam = await _context.ExamQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var questionUsingForScript = await _context.ScriptQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var freeQuestions = await _context.Questions.AsNoTracking().Select(sel => sel.Id).ToListAsync();
            freeQuestions = freeQuestions.Where(q => !questionUsingForExam.Any(id => q == id) && !questionUsingForQuiz.Any(id => id == q ) && !questionUsingForScript.All(id => id == q)).ToList();
            return new StatisticalDTO{
                Free = freeQuestions.Count,
                QuizUsing = questionUsingForQuiz.Count,
                ExamUsing = questionUsingForExam.Count,
                ScriptUsing = questionUsingForScript.Count
            };
        }

        public async Task<bool> CreateQuestionAsync(QuestionCreateDTO questionCreateDTO)
        {
            var question = _mapper.Map<Question>(questionCreateDTO);
            if(questionCreateDTO.Image != null){
                question.ImageFileName = _fileService.UploadFile(questionCreateDTO.Image,SD.ImagePath);
            }
            if(questionCreateDTO.Audio != null){
                question.AudioFileName = _fileService.UploadFile(questionCreateDTO.Audio,SD.AudioPath);
            }
            _context.Questions.Add(question);
            if(await _context.SaveChangesAsync() > 0){
                return true;
            }
            return false;
        }

        public Task<bool> CheckExistAsync(Question question)
        {
            throw new NotImplementedException();
        }
    }
}