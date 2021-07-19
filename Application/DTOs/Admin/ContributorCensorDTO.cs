using Application.DTOs.Example;
using Application.DTOs.Pagination;

namespace Application.DTOs.Admin
{
    public class ContributorCensorDTO
    {
        public PaginateDTO<ExampleDTO> ExampleContributors { get; set; }
    }
}