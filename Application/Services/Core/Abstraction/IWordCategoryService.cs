using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Word.WordCategory;
using Domain.Models.Version2;
namespace Application.Services.Core.Abstraction
{
    public interface IWordCategoryService
    {
        Task<bool> ExistAsync(Guid id);
        Task<PaginateDTO<WordCategory>> GetAllAsync(PaginationDTO pagination, bool manage, string search = null, bool learn = false, string tag = "all");
        Task<List<WordCategoryDTO>> GetAllAsync();
        Task<WordCategoryDTO> GetDetailAsync(Guid id);
        Task<WordCategoryDTO> UserGetDetailAsync(int accountId, Guid categoryId);
        Task<List<QuestionDTO>> GetPracticeQuestion(Guid categoryId);
        Task<bool> CheckConflictAsync(string name);
        Task<WordCategoryDTO> CreateCategoryAsync(WordCategoryCreateDTO wordCategoryCreateDTO);
        Task<WordCategoryDTO> UpdateCategoryAsync(Guid id, WordCategoryCreateDTO wordCategoryUpdateDTO);
        Task DeleteCategoryAsync(Guid id);
    }
}