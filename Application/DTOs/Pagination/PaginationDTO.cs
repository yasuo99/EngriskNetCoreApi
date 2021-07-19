namespace Application.DTOs.Pagination
{
    public class PaginationDTO
    {
        public int PageSize { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
    }
}