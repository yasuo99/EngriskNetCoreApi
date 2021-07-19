using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Engrisk.Helper
{
    public class PagingList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public PagingList(List<T> items, int currentPage, int pageSize, int totalItems)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems/(double)pageSize);
            this.AddRange(items);
        }
        public static async Task<PagingList<T>> OnCreateAsync(IQueryable<T> items, int currentPage, int pageSize)
        {
            var totalItems = await items.CountAsync();
            var returnItems = await items.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagingList<T>(returnItems, currentPage, pageSize, totalItems);
        }
    }
}