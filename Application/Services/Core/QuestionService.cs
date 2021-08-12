using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Answer;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            var lines = new List<ConversationAnswerDTO>();
            foreach (var answer in question.Answers)
            {
                var line = JsonConvert.DeserializeObject<ConversationAnswerDTO>(answer.Content);
                lines.Add(line);
            }
            var hiddenAnswer = new List<string>();
            foreach (var line in lines)
            {
                if (line.FirstLine.Contains("<p>"))
                {
                    hiddenAnswer.Add(line.FirstLine);
                }
                if (line.SecondLine.Contains("<p>"))
                {
                    hiddenAnswer.Add(line.SecondLine);
                }
            }
            if (hiddenAnswer.Count != splittedAnswers.Count())
            {
                return false;
            }
            for (int i = 0; i < hiddenAnswer.Count; i++)
            {
                if (!hiddenAnswer[i].Contains(splittedAnswers[i]))
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
                }
                else
                {
                    _context.DayStudies.Add(new Domain.Models.Version2.DayStudy
                    {
                        Date = DateTime.Now.Date,
                        AccountId = accountId
                    });
                }

                _context.Add(wordLearnt);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PaginateDTO<QuestionDTO>> GetQuestionsAsync(PaginationDTO pagination, QuestionType type, QuestionStatus status, GrammarQuestionType grammar, string search = null)
        {
            var questions = from q in _context.Questions.OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate)
                            select q;
            if (search != null)
            {
                questions = questions.Where(q => (!string.IsNullOrEmpty(q.PreQuestion) && q.PreQuestion.ToLower().Contains(search.ToLower()) || (!string.IsNullOrEmpty(q.Content) && q.Content.ToLower().Contains(search.ToLower()))));
            }
            if (status != QuestionStatus.None)
            {
                questions = questions.Where(question => question.Status == status);
            }
            if (type != QuestionType.None)
            {
                questions = questions.Where(question => question.Type == type);
            }
            if (grammar != GrammarQuestionType.None)
            {
                questions = questions.Where(question => question.Grammar == grammar);
            }
            var paginglistQuestions = await PagingList<Question>.OnCreateAsync(questions, pagination.CurrentPage, pagination.PageSize);
            var result = paginglistQuestions.CreatePaginate();
            var questionsDto = _mapper.Map<List<QuestionDTO>>(result.Items);
            foreach (var item in questionsDto)
            {
                item.Answers = _mapper.Map<List<AnswerDTO>>(await _context.Answers.Where(ans => ans.QuestionId == item.Id).ToListAsync());
            }
            return new PaginateDTO<QuestionDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                TotalPages = result.TotalPages,
                TotalItems = result.TotalItems,
                Items = questionsDto
            };
        }

        public async Task<List<QuestionDTO>> GetQuestionsAsync(QuestionType type, GrammarQuestionType grammar, string search = null)
        {
            var questions = await _context.Questions.Include(inc => inc.Answers).AsNoTracking().ToListAsync();
            if (type != QuestionType.None)
            {
                if (type == QuestionType.Quiz)
                {
                    questions = questions.Where(question => question.Type != QuestionType.Toeic).ToList();
                }
                else
                {
                    questions = questions.Where(question => question.Type == type).ToList();
                }
            }
            if (grammar != GrammarQuestionType.None)
            {
                questions = questions.Where(question => question.Grammar == grammar).ToList();
            }
            if (search != null)
            {
                questions = questions.Where(q => (!string.IsNullOrEmpty(q.PreQuestion) && q.PreQuestion.ToLower().Contains(search.ToLower()) || (!string.IsNullOrEmpty(q.Content) && q.Content.ToLower().Contains(search.ToLower())))).ToList();
            }
            return _mapper.Map<List<QuestionDTO>>(questions);
        }

        public async Task<StatisticalDTO> GetStatisticalAsync()
        {
            var questionUsingForQuiz = await _context.QuizQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var questionUsingForExam = await _context.ExamQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var questionUsingForScript = await _context.ScriptQuestions.AsNoTracking().Select(sel => sel.QuestionId).ToListAsync();
            var totalQuestion = await _context.Questions.AsNoTracking().ToListAsync();
            return new StatisticalDTO
            {
                Total = totalQuestion.Count,
                Free = totalQuestion.Count(q => q.Status == QuestionStatus.Free),
                QuizUsing = questionUsingForQuiz.Count,
                ExamUsing = questionUsingForExam.Count,
                ScriptUsing = questionUsingForScript.Count
            };
        }

        public async Task<Question> CreateQuestionAsync(QuestionCreateDTO questionCreateDTO)
        {
            var question = _mapper.Map<Question>(questionCreateDTO);
            if (questionCreateDTO.Image != null)
            {
                question.ImageFileName = _fileService.UploadFile(questionCreateDTO.Image, SD.ImagePath);
            }
            if (questionCreateDTO.Audio != null)
            {
                question.AudioFileName = _fileService.UploadFile(questionCreateDTO.Audio, SD.AudioPath);
            }
            _context.Questions.Add(question);
            if (await _context.SaveChangesAsync() > 0)
            {
                return question;
            }
            return null;
        }

        public async Task<bool> UpdateQuestionAsync(Guid id, QuestionCreateDTO questionCreateDTO)
        {
            var question = await _context.Questions.Include(inc => inc.Answers).FirstOrDefaultAsync(q => q.Id == id);
            _mapper.Map(questionCreateDTO, question);
            if (questionCreateDTO.Image != null)
            {
                _fileService.DeleteFile(question.ImageFileName);
                question.ImageFileName = _fileService.UploadFile(questionCreateDTO.Image, SD.ImagePath);
            }
            if (questionCreateDTO.Audio != null)
            {
                _fileService.DeleteFile(question.AudioFileName);
                question.AudioFileName = _fileService.UploadFile(questionCreateDTO.Audio, SD.AudioPath);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
        public Task<bool> CheckExistAsync(Question question)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteQuestionAsync(Guid id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
            _context.Remove(question);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckExistAsync(Guid id)
        {
            return await _context.Questions.AnyAsync(q => q.Id == id);
        }

        public void ChangeStatusAsync(Question question)
        {
            question.Status = question.Status == QuestionStatus.Free ? QuestionStatus.InUse : QuestionStatus.Free;
        }

        public void ChangeStatusAsync(List<Question> questions)
        {
            foreach (var question in questions)
            {
                ChangeStatusAsync(question);
            }
        }

        public async Task ChangeStatusAsync(Guid id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
            ChangeStatusAsync(question);
        }

        public async Task ChangeStatusAsync(List<Guid> ids)
        {
            foreach (var id in ids)
            {
                await ChangeStatusAsync(id);
            }
        }

        public Task<bool> ImportFromFile(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public async Task CreateVocabularyPracticeQuestion(Word word)
        {
            var basicQuestion = await CreateBasicQuestionOfVocabulary(word);
            _context.Questions.Add(basicQuestion);
            word.Questions.Add(new WordQuestion
            {
                Question = basicQuestion
            });
            var arrangeQuestion = CreateArrangeQuestionOfVocabulary(word);
            _context.Questions.Add(arrangeQuestion);
            word.Questions.Add(new WordQuestion
            {
                Question = arrangeQuestion
            });
            var filloutQuestion = CreateFilloutQuestionOfVocabulary(word);
            _context.Questions.Add(filloutQuestion);
            word.Questions.Add(new WordQuestion
            {
                Question = filloutQuestion
            });
            var listeningQuestion = CreateListeningQuestionOfVocabulary(word);
            _context.Questions.Add(listeningQuestion);
            word.Questions.Add(new WordQuestion
            {
                Question = listeningQuestion
            });
            Question speakingQuestion = CreateSpeakingQuestionOfVocabulary(word);
            _context.Questions.Add(speakingQuestion);
            word.Questions.Add(new WordQuestion
            {
                Question = speakingQuestion
            });
        }

        private Question CreateSpeakingQuestionOfVocabulary(Word word)
        {
            var speakingQuestion = new Question
            {
                PreQuestion = $"Click micro và phát âm từ",
                Content = $"{word.Eng} : {word.Spelling}",
                Type = QuestionType.Speaking,
                Status = QuestionStatus.InUse
            };
            speakingQuestion.Answers.Add(new Answer
            {
                Content = $"{word.Eng}.",
                IsQuestionAnswer = true
            });
            return speakingQuestion;
        }

        private Question CreateFilloutQuestionOfVocabulary(Word word)
        {
            var filloutQuestion = new Question
            {
                PreQuestion = "Điền từ có nghĩa là",
                Content = $"{word.Vie}",
                Type = QuestionType.Fillout,
                Status = QuestionStatus.InUse
            };
            filloutQuestion.Answers.Add(new Answer
            {
                Content = word.Eng,
                IsQuestionAnswer = true
            });
            filloutQuestion.Answers.Add(new Answer
            {
                Content = word.Eng.ToLower(),
                IsQuestionAnswer = true
            });
            _context.Add(filloutQuestion);
            return filloutQuestion;
        }
        private Question CreateListeningQuestionOfVocabulary(Word word)
        {
            var filloutQuestion = new Question
            {
                PreQuestion = "Nghe và điền từ vựng vào chỗ trống",
                AudioFileName = word.WordVoice,
                Type = QuestionType.Fillout,
                Status = QuestionStatus.InUse
            };
            filloutQuestion.Answers.Add(new Answer
            {
                Content = word.Eng,
                IsQuestionAnswer = true
            });
            filloutQuestion.Answers.Add(new Answer
            {
                Content = word.Eng.ToLower(),
                IsQuestionAnswer = true
            });
            _context.Add(filloutQuestion);
            return filloutQuestion;
        }

        private Question CreateArrangeQuestionOfVocabulary(Word word)
        {
            var arrangeQuestion = new Question
            {
                Content = word.Eng,
                PreQuestion = $"Sắp xếp thành từ có nghĩa sau: {word.Vie}",
                Type = QuestionType.Arrange,
                Status = QuestionStatus.InUse
            };
            _context.Add(arrangeQuestion);
            return arrangeQuestion;
        }

        private async Task<Question> CreateBasicQuestionOfVocabulary(Word word)
        {
            var basicQuestion = new Question
            {
                Content = $"Từ <b>{word.Eng}</b> có nghĩa là gì?",
                PreQuestion = $"Chọn đáp án đúng",
                Type = QuestionType.Basic,
                Status = QuestionStatus.InUse
            };
            var answers = await _context.Word.Where(w => w.Type == VocabularyType.Insert && !string.IsNullOrEmpty(w.Vie)).Take(3).ToListAsync();
            answers.Add(word);
            foreach (var answer in answers.Shuffle())
            {
                basicQuestion.Answers.Add(new Answer
                {
                    Content = answer.Vie,
                    IsQuestionAnswer = answer.Eng == word.Eng ? true : false
                });
            }
            _context.Add(basicQuestion);
            return basicQuestion;
        }

        public async Task CreateVocabularyPracticeQuestion(Word word, Example example)
        {
            Question exampleArrangeQuestion = CreateVocabularyExampleArrangeQuestion(example);
            word.Questions.Add(new WordQuestion
            {
                Question = exampleArrangeQuestion
            });
            Question exampleSpeakingQuestion = CreateVocabularyExampleSpeakingQuestion(example);
            word.Questions.Add(new WordQuestion
            {
                Question = exampleSpeakingQuestion
            });
        }

        private Question CreateVocabularyExampleArrangeQuestion(Example example)
        {
            var question = new Question
            {
                PreQuestion = $"Sắp xếp câu sau theo thứ tự chính xác với nghĩa {example.Vie}",
                Content = $"{example.Eng}",
                Type = QuestionType.Arrange,
                Status = QuestionStatus.InUse
            };
            _context.Add(question);
            return question;
        }
        private Question CreateVocabularyExampleSpeakingQuestion(Example example)
        {
            var question = new Question
            {
                PreQuestion = $"Chọn micro và phát âm câu sau",
                Content = $"{example.Eng}",
                Type = QuestionType.Speaking,
                Status = QuestionStatus.InUse
            };
            question.Answers.Add(new Answer
            {
                Content = $"{example.Eng}.",
                IsQuestionAnswer = true
            });
            _context.Add(question);
            return question;
        }

        public async Task<QuestionDTO> GetQuestionAsync(Guid id)
        {
            var question = await _context.Questions.Where(q => q.Id == id).Include(inc => inc.Answers).FirstOrDefaultAsync();
            return _mapper.Map<QuestionDTO>(question);
        }

        public async Task FillVocabularyPracticeQuestion(Word word)
        {
            if(!word.Questions.Any(q => q.Question.Type == QuestionType.Basic)){
                var question = await CreateBasicQuestionOfVocabulary(word);
                word.Questions.Add(new WordQuestion{
                    Question = question
                });
            }
             if(!word.Questions.Any(q => q.Question.Type == QuestionType.Fillout)){
                var question = CreateFilloutQuestionOfVocabulary(word);
                word.Questions.Add(new WordQuestion{
                    Question = question
                });
            }
             if(!word.Questions.Any(q => q.Question.Type == QuestionType.Speaking)){
                var question =  CreateSpeakingQuestionOfVocabulary(word);
                word.Questions.Add(new WordQuestion{
                    Question = question
                });
            }
            if(!word.Questions.Any(q => q.Question.Type == QuestionType.Arrange)){
                var question =  CreateArrangeQuestionOfVocabulary(word);
                word.Questions.Add(new WordQuestion{
                    Question = question
                });
            }
            if(!word.Questions.Any(q => q.Question.Type == QuestionType.Listening)){
                var question =  CreateListeningQuestionOfVocabulary(word);
                word.Questions.Add(new WordQuestion{
                    Question = question
                });
            }
        }
    }
}