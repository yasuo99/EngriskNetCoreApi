using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services.Core
{
    public class CategoryTagService : ICategoryTagService
    {
        private readonly ApplicationDbContext _context;

        public CategoryTagService()
        {
        }

        public CategoryTagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> CreateCategoryTagAsync(CategoryTag categoryTag)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteCategoryTagAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginateDTO<CategoryTag>> GetAllCategoryTagAsync(PaginationDTO pagination, string search = null)
        {
            var categoryTags = await _context.CategoryTags.AsNoTracking().ToListAsync();
            if(search != null){
                categoryTags = categoryTags.Where(ct => ct.Tag.ToLower().Contains(search.Trim().ToLower())).ToList();
            }
            var pagingListTags = PagingList<CategoryTag>.OnCreate(categoryTags, pagination.CurrentPage,pagination.PageSize);
            return pagingListTags.CreatePaginate();
        }

        public async Task<List<CategoryTag>> GetAllCategoryTagAsync()
        {
            var tags = await _context.CategoryTags.AsNoTracking().ToListAsync();
            return tags;
        }

        public Task<bool> UpdateCategoryTagAsync(Guid id, CategoryTag categoryTag)
        {
            throw new NotImplementedException();
        }
    }
}