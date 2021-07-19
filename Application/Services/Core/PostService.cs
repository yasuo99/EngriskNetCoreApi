using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Post;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Z.EntityFramework.Plus;

namespace Application.Services.Core
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PostService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CensorContentAsync(Guid id, Status status)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
            post.VerifiedStatus = status;
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<PaginateDTO<PostDTO>> GetAllPosts(PaginationDTO pagination, Status status = Status.Approved, string search = null)
        {
            var posts = await _context.Posts.Where(post => post.VerifiedStatus == status).Include(inc => inc.Account).Include(inc => inc.PostImages).Include(inc => inc.Comments).ToListAsync();
            if(search != null){
                posts = posts.Where(p => p.Title.ToLower().Contains(search.Trim().ToLower())  || p.Content.ToLower().Contains(search.Trim().ToLower())).ToList();
            }
            var postsDto = _mapper.Map<List<PostDTO>>(posts);
            var pagingListPosts = PagingList<PostDTO>.OnCreate(postsDto, pagination.CurrentPage, pagination.PageSize);
            return pagingListPosts.CreatePaginate();
        }

        public async Task<List<PostDTO>> GetAllPosts(PostTypes type, int acccountId = 0, Status status = Status.Approved)
        {
            switch (type)
            {
                case PostTypes.Hot:
                    var hotPosts = await _context.Posts.Where(post => post.VerifiedStatus == status).Include(inc => inc.Account).IncludeFilter(inc => inc.Comments.Where(comment => comment.VerifiedStatus == Status.Approved)).OrderByDescending(orderBy => orderBy.Comments.Count).Take(10).ToListAsync();
                    var hotPostsDto = _mapper.Map<List<PostDTO>>(hotPosts);
                    return hotPostsDto;
                case PostTypes.New:
                    var newPosts = await _context.Posts.Where(post => post.VerifiedStatus == status).Include(inc => inc.Account).Include(inc => inc.Comments).OrderByDescending(orderBy => orderBy.CreatedDate).Take(10).ToListAsync();
                    var newPostsDto = _mapper.Map<List<PostDTO>>(newPosts);
                    return newPostsDto;
                case PostTypes.Following:
                    var followingPosts = await _context.Posts.Where(post => post.VerifiedStatus == status && post.Comments.Any(comment => comment.AccountId == acccountId)).Include(inc => inc.Account).Include(inc => inc.Comments).OrderByDescending(orderBy => orderBy.Comments.Count).Take(10).ToListAsync();
                    var followingPostsDto = _mapper.Map<List<PostDTO>>(followingPosts);
                    return followingPostsDto;
                default:
                    return null;
            }
        }
        public async Task<Comment> GetComment(Guid id)
        {
            return await _context.Comments.Where(comment => comment.Id == id).Include(inc => inc.Reply).Include(inc => inc.Replies).Include(inc => inc.Account).FirstOrDefaultAsync();
        }

        public async Task<PostDTO> GetPostDetailAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task ReportComment(Comment comment)
        {
            var report = new Spam
            {
                CommentId = comment.Id,
                Author = comment.Account.UserName,
                Content = comment.Content,
                Timestamp = DateTime.Now,
                Status = Domain.Enums.Status.Pending
            };
            _context.Spams.Add(report);
        }
    }
}