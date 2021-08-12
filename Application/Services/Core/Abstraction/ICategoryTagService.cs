using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface ICategoryTagService
    {
        Task<bool> CheckExistAsync(Guid id);
        Task<List<CategoryTag>> GetAllCategoryTagAsync();
        Task<PaginateDTO<CategoryTag>> GetAllCategoryTagAsync(PaginationDTO pagination, string search = null);
        Task<bool> CreateCategoryTagAsync(CategoryTag categoryTag);
        Task<bool> UpdateCategoryTagAsync(Guid id, CategoryTag categoryTag);
        Task<bool> DeleteCategoryTagAsync(Guid id);
    }
}