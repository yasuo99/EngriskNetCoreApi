using System.Collections.Generic;
using Application.Helper;

namespace Application.DTOs.Pagination
{
    public class PaginateDTO<T>
    {
        private int currentPage;
        public int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }
        private int pageSize;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }
        private int totalPages;
        public int TotalPages
        {
            get { return totalPages; }
            set { totalPages = value; }
        }
        private int totalItems;
        public int TotalItems
        {
            get { return totalItems; }
            set { totalItems = value; }
        }
        private List<T> items;
        public List<T> Items
        {
            get { return items; }
            set { items = value; }
        }
        private int itemsOnPage;
        public int ItemsOnPage
        {
            get { return itemsOnPage; }
            set { itemsOnPage = value; }
        }
        
        public PaginateDTO()
        {
        }
        public PaginateDTO(int currentPage, int pageSize, int totalPages, int totalItems, int itemsOnPage, PagingList<T> items)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            TotalItems = totalItems;
            ItemsOnPage = itemsOnPage;
            Items = items;
        }
    }
}