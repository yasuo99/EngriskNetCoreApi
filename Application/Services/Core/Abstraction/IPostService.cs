using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Post;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface IPostService : ICensorService
    {
        Task<PaginateDTO<PostDTO>> GetAllPosts(PaginationDTO pagination, Status status = Status.Approved, string search = null);
        Task<List<PostDTO>> GetAllPosts(PostTypes type, int acccountId = 0, Status status = Status.Approved);
        Task<PostDTO> GetPostDetailAsync(Guid id);
        Task ReportComment(Comment comment);
        Task<Comment> GetComment(Guid id);
    }
}