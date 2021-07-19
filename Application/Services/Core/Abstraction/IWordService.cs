using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Memory;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Word;
using Domain.Models;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface IWordService
    {
        Task<PaginateDTO<WordDTO>> GetAllAsync(PaginationDTO pagination, string search = null);
        Task<List<WordDTO>> GetAllAsync();
        Task<List<QuestionDTO>> GetVocabularyPracticeQuestionsAsync(Guid id);
        Task<Word> GetDetailAsync(Guid id);
        Task<WordDTO> GetWordAsync(Guid id);
        Task<bool> AddWordQuestionAsync(Guid id, List<QuestionDTO> questions);
        Task<List<WordDTO>> SuggestWord(string content);
        Task<List<WordDTO>> SearchWordAsync(string content);
        Task<Word> GetDetailWithMemoriesAsync(Guid id);
        Task<bool> CheckExistAsync(Guid id);
        Task<bool> CheckConflictAsync(WordCreateDTO wordCreateDTO);
        Task<WordDTO> CreateWordAsync(WordCreateDTO wordCreateDTO);
        Task<Memory> CreateWordMemoryAsync(Word word, MemoryCreateDTO memoryCreateDTO, int accountId);
        Task<Memory> GetMemoryAsync(Word word, Guid memoryId);
        Task SelectMemoryAsync(int accountId, Guid wordId, Guid memoryId);
        Task<bool> DeleteMemoryAsync(int accountId, Guid wordId, Guid memoryId);
        Task<Question> CreatePracticeQuestionAsync(Guid wordId, QuestionCreateDTO questionCreateDTO);
        Task DeleteMemoryAsync(Memory memory);
        Task DeleteMemoryAsync(Guid wordId, Guid memoryId);
        Task<Word> UpdateAsync(Guid id, WordUpdateDTO wordUpdateDTO);
        Task DeleteWordAsync(Guid id);
        Task DeleteWordAsync(Word word);
        Task<Example> ContributeExampleAsync(Guid wordId, Example example);
        Task CreateWordQuestions(Guid wordId, WordQuestionsCreateDTO wordQuestionsCreateDTO);
        Task<List<WordLearntDTO>> GetWordLearneds(int id);
        Task<string> TranslateAsync(string content);
        Task<List<WordDTO>> GetVocabularyForScriptAsync(string search = null);

        //Ôn tập từ vựng
        Task<List<QuestionDTO>> WordLearnedReviewAsync(int accountId, string option);
        Task<List<QuestionDTO>> VocabularyReviewAsync(List<Guid> words);
        Task<bool> CheckReviewQuestionAsync(Guid questionId, Guid answerId);
    }
}