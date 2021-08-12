using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Application.DTOs.Comment;
using Application.DTOs.Post;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models.Version2;
using Application.Services;
using Application.Utilities;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;
using Application.DTOs.Notification;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IHubContext<NotificationHub> _hub;
        public PostsController(ICRUDRepo repo, IMapper mapper, IFileService fileService, IHubContext<NotificationHub> hub)
        {
            _mapper = mapper;
            _repo = repo;
            _fileService = fileService;
            _hub = hub;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] SubjectParams subjectParams)
        {
            var posts = await _repo.GetAll<Post>(subjectParams, null, "Account, Comments");
            var returnPosts = _mapper.Map<IEnumerable<PostDTO>>(posts);
            return Ok(returnPosts);
        }
        [HttpGet("rating")]
        public async Task<IActionResult> GetHighRatePosts([FromQuery] SubjectParams subjectParams)
        {
            var posts = await _repo.GetAll<Post>(subjectParams, null, "Account");
            var returnPosts = _mapper.Map<IEnumerable<PostDTO>>(posts);
            return Ok(returnPosts);
        }
        [HttpGet("new")]
        public async Task<IActionResult> GetNewPosts([FromQuery] SubjectParams subjectParams)
        {
            var posts = await _repo.GetAll<Post>(subjectParams, null, "Account", order => order.OrderByDescending(post => post.CreatedDate));
            var returnPosts = _mapper.Map<IEnumerable<PostDTO>>(posts);
            return Ok(returnPosts);
        }
        [Authorize]
        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingPosts([FromQuery] SubjectParams subjectParams)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _repo.GetAll<Post>(subjectParams);
            var returnPosts = _mapper.Map<IEnumerable<PostDTO>>(posts);
            return Ok(returnPosts);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] SubjectParams subjectParams, [FromBody] SearchDTO searchDTO)
        {
            var postsFromDb = await _repo.GetAll<Post>(subjectParams, post => post.Title.ToLower().Contains(searchDTO.Search.ToLower().Trim()) || post.Content.ToLower().Contains(searchDTO.Search.ToLower().Trim()), "Account");
            var returnPosts = _mapper.Map<IEnumerable<PostDTO>>(postsFromDb);
            return Ok(returnPosts);
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(Guid id, [FromQuery] SubjectParams subjectParams, [FromQuery] string orderBy = "newest")
        {
            int accountId = 0;
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            var postQuery = await _repo.GetOneWithManyToMany<Post>(pos => pos.Id == id);
            var post = await postQuery.Include(inc => inc.Account).Include(inc => inc.PostImages).FirstOrDefaultAsync();
            post.AccessCount += 1;
            await _repo.SaveAll();
            if (accountId == 0)
            {
                post.Comments = await _repo.GetAll<Comment>(comment => comment.VerifiedStatus == Domain.Enums.Status.Approved && comment.PostId == id, includeProperties: "Account", orderBy => orderBy.OrderByDescending(order => order.Timestamp));
                foreach (var comment in post.Comments.ToList())
                {
                    comment.Replies = await _repo.GetAll<Comment>(reply => reply.ReplyId == comment.Id && reply.VerifiedStatus == Domain.Enums.Status.Approved, includeProperties: "Account",orderBy => orderBy.OrderByDescending(order => order.Timestamp));
                }
            }
            else
            {
                post.Comments = await _repo.GetAll<Comment>(comment => comment.PostId == id && (comment.VerifiedStatus == Domain.Enums.Status.Approved  || comment.AccountId == accountId && comment.VerifiedStatus == Domain.Enums.Status.Pending) , includeProperties: "Account",orderBy => orderBy.OrderByDescending(order => order.Timestamp));
                foreach (var comment in post.Comments.ToList())
                {
                    comment.Replies = await _repo.GetAll<Comment>(reply => reply.ReplyId == comment.Id && (reply.VerifiedStatus == Domain.Enums.Status.Approved || (reply.AccountId == accountId && reply.VerifiedStatus == Domain.Enums.Status.Pending)), includeProperties: "Account",orderBy => orderBy.OrderByDescending(order => order.Timestamp));
                }
            }
            if (post == null)
            {
                return NotFound();
            }
            var returnPost = _mapper.Map<PostDetailDTO>(post);
            switch (orderBy)
            {
                case "like":
                    returnPost.Comments = returnPost.Comments.Where(c => c.ReplyId == Guid.Empty).OrderByDescending(c => c.Like).ToList();
                    break;
                case "newest":
                    returnPost.Comments = returnPost.Comments.Where(c => c.ReplyId == Guid.Empty).OrderByDescending(c => c.Date).ToList();
                    break;
                case "oldest":
                    returnPost.Comments = returnPost.Comments.Where(c => c.ReplyId == Guid.Empty).OrderBy(c => c.Date).ToList();
                    break;
                default: break;
            }
            return Ok(returnPost);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDTO postCreate)
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var accountFromDb = await _repo.GetOneWithConditionTracking<Account>(acc => acc.Id == accountId);
            if (accountFromDb.Locked > DateTime.Now)
            {
                return BadRequest(new
                {
                    Error = "Tài khoản của bạn đang bị khóa chức năng thảo luận"
                });
            }
            var post = _mapper.Map<Post>(postCreate);
            post.AccountId = accountId;
            post.Account = accountFromDb;
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Content", post.Content);
            properties.Add("Title", post.Title);
            properties.Add("AccountId", accountId);
            if (_repo.Exists<Post>(properties))
            {
                return Conflict(new
                {
                    Error = "Bài viết bị trùng"
                });
            }
            post.CreatedDate = DateTime.Now;
            post.Title = await CensoredString(post.Title);
            post.Content = await CensoredString(post.Content);
            var lastCreatedPost = await _repo.GetOneWithCondition<Post>(p => p.AccountId == accountId && p.CreatedDate.AddMinutes(15) > DateTime.Now);
            if (lastCreatedPost != null)
            {
                return BadRequest(new
                {
                    Error = "Không được phép đăng liên tiếp bài viết"
                });
            }
            if (postCreate.Images != null)
            {
                if (postCreate.Images.Count > 0)
                {
                    foreach (var image in postCreate.Images)
                    {
                        post.PostImages.Add(new PostImage
                        {
                            ImageUrl = _fileService.UploadFile(image, SD.ImagePath),
                            FileName = image.FileName
                        });
                    }
                }
            }

            if (User.IsInRole("superadmin") || User.IsInRole("manager"))
            {
                post.VerifiedStatus = Domain.Enums.Status.Approved;
            }
            _repo.Create(post);
            if (await _repo.SaveAll())
            {
                var postDTO = _mapper.Map<PostDTO>(post);
                foreach (var client in HubHelper.NotificationClientsConnections)
                {
                    await _hub.Clients.Client(client.ClientId).SendAsync("NewPost", Extension.CamelCaseSerialize(postDTO));
                }
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> CommentToPost(Guid id, [FromBody] CommentDTO comment)
        {
            if (await ValidateString(comment.Comment))
            {
                return BadRequest(new
                {
                    validate = "Comment có chứa quá nhiều kí tự nhạy cảm"
                });
            }
            comment.Comment = await CensoredString(comment.Comment);
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var accountFromDb = await _repo.GetOneWithConditionTracking<Account>(acc => acc.Id == userId);
            if (DateTime.Compare(accountFromDb.Locked, DateTime.Now) > 0)
            {
                return BadRequest(new
                {
                    error = "Tài khoản tạm thời bị khóa bình luận"
                });
            }
            var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            var commentToPost = new Comment()
            {
                PostId = id,
                AccountId = userId,
                Content = comment.Comment,
                Timestamp = DateTime.Now,
            };
            if (User.IsInRole("superadmin") || User.IsInRole("admin"))
            {
                commentToPost.VerifiedStatus = Domain.Enums.Status.Approved;
            }
            commentToPost.Account = accountFromDb;
            _repo.Create(commentToPost);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [Authorize]
        [HttpPost("{postId}/comments/{commentId}")]
        public async Task<IActionResult> ReplyComment(Guid postId, Guid commentId, [FromBody] CommentReplyDTO commentReplyDTO)
        {
            try
            {
                var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == postId, "Comments");
                if (postFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy bài viết"
                    });
                }
                if (!postFromDb.Comments.Any(c => c.Id == commentId))
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy bình luận"
                    });
                }
                if (await ValidateString(commentReplyDTO.Content))
                {
                    return BadRequest(new
                    {
                        validate = "Comment có chứa quá nhiều kí tự nhạy cảm"
                    });
                }
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var accountFromDb = await _repo.GetOneWithConditionTracking<Account>(acc => acc.Id == userId);
                if (accountFromDb.Locked > DateTime.Now)
                {
                    return BadRequest(new
                    {
                        Error = "Tài khoản của bạn đang bị khóa chức năng thảo luận"
                    });
                }
                var comment = new Comment()
                {
                    ReplyId = commentId,
                    PostId = postId,
                    AccountId = userId,
                    Content = commentReplyDTO.Content,
                    Timestamp = DateTime.Now,
                };
                comment.Account = accountFromDb;
                _repo.Create(comment);
                if (await _repo.SaveAll())
                {
                    var commentDTO = _mapper.Map<CommentDTO>(comment);
                    var client = HubHelper.NotificationClientsConnections.FirstOrDefault(client => client.AccountId == accountFromDb.Id);
                    if (client != null)
                    {
                        await _hub.Clients.Client(client.ClientId).SendAsync("NewReplyComment", Extension.CamelCaseSerialize(commentDTO));
                    }
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
        [Authorize]
        [HttpPut("{postId}/comments/{commentId}")]
        public async Task<IActionResult> EditReplyComment(Guid postId, Guid commentId, [FromBody] CommentReplyDTO commentReplyDTO)
        {
            try
            {
                var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == postId, "Comments");
                if (postFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy bài viết"
                    });
                }
                if (!postFromDb.Comments.Any(c => c.Id == commentId))
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy bình luận"
                    });
                }
                if (await ValidateString(commentReplyDTO.Content))
                {
                    return BadRequest(new
                    {
                        validate = "Comment có chứa quá nhiều kí tự nhạy cảm"
                    });
                }
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var accountFromDb = await _repo.GetOneWithConditionTracking<Account>(acc => acc.Id == userId);
                if (accountFromDb.Locked > DateTime.Now)
                {
                    return BadRequest(new
                    {
                        Error = "Tài khoản của bạn đang bị khóa chức năng thảo luận"
                    });
                }
                var commentFromDb = await _repo.GetOneWithConditionTracking<Comment>(c => c.Id == commentId, includeProperties: "Replies");
                if (userId != commentFromDb.AccountId && (!User.IsInRole("forumadmin") || !User.IsInRole("manager") || !User.IsInRole("superadmin")))
                {
                    return Unauthorized();
                }
                commentFromDb.Content = commentReplyDTO.Content;
                if (await _repo.SaveAll())
                {
                    var commentDTO = _mapper.Map<CommentDTO>(commentFromDb);
                    foreach (var client in HubHelper.NotificationClientsConnections)
                    {
                        await _hub.Clients.Client(client.ClientId).SendAsync("UpdateComment", Extension.CamelCaseSerialize(commentDTO));
                    }
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
        [Authorize]
        [HttpPut("{postId}/comments/{commentId}/like")]
        public async Task<IActionResult> LikeComment(Guid postId, Guid commentId)
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == postId);
            if (postFromDb == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy bài viết"
                });
            }
            var commentOfPost = await _repo.GetOneWithConditionTracking<Comment>(comment => comment.PostId == postId && comment.Id == commentId);
            if (commentOfPost == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy bình luận"
                });
            }
            if (commentOfPost.AccountId == accountId)
            {
                return NoContent();
            }
            var likedComment = await _repo.GetOneWithCondition<LikedComment>(c => c.CommentId == commentOfPost.Id && c.AccountId == accountId);
            if (likedComment == null)
            {
                var likeComment = new LikedComment()
                {
                    AccountId = accountId,
                    CommentId = commentOfPost.Id,
                    Timestamp = DateTime.Now
                };
                _repo.Create(likeComment);
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
            }
            else
            {
                _repo.Delete(likedComment);
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
            }
            return NoContent();
        }
        [Authorize]
        [HttpDelete("{id}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == id, "Comments");
            if (postFromDb == null)
            {
                return NotFound(new
                {
                    NotFound = "Không tìm thấy bài viết"
                });
            }
            var commentFromDb = await _repo.GetOneWithConditionTracking<Comment>(comment => comment.Id == commentId, "Replies,LikedComments");
            if (commentFromDb == null)
            {
                return NotFound(new
                {
                    NotFound = "Không tìm thấy bình luận"
                });
            }
            if (userId != commentFromDb.AccountId && !User.IsInRole("forumadmin") && !User.IsInRole("manager") && !User.IsInRole("superadmin"))
            {
                return Unauthorized();
            }
            if (commentFromDb.Replies.Count() > 0)
            {
                foreach (var reply in commentFromDb.Replies)
                {
                    var comment = await _repo.GetOneWithCondition<Comment>(c => c.Id == reply.ReplyId, "LikedComments");
                    _repo.Delete(comment);
                    _repo.Delete(reply);
                }
            }
            // if (commentFromDb.LikedComments.Count() > 0)
            // {
            //     foreach (var like in commentFromDb.LikedComments)
            //     {
            //         _repo.Delete(like);
            //     }
            // }
            _repo.Delete(commentFromDb);
            if (await _repo.SaveAll())
            {
                foreach (var client in HubHelper.NotificationClientsConnections)
                {
                    await _hub.Clients.Client(client.ClientId).SendAsync("DeleteComment", commentId);
                }
                return Ok();
            }
            return StatusCode(500);
        }
        [Authorize]
        [HttpPost("{id}/rating")]
        public async Task<IActionResult> UpDownVotePost(Guid id, [FromBody] RatingDTO rating)
        {
            if (id != rating.Id)
            {
                return NotFound();
            }
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var post = await _repo.GetOneWithCondition<Post>(post => post.Id == id);

            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] PostUpdateDTO post)
        {
            try
            {
                var postFromDb = await _repo.GetOneWithConditionTracking<Post>(post => post.Id == id);
                if (postFromDb == null)
                {
                    return NotFound();
                }
                _mapper.Map(post, postFromDb);
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception e)
            {

                throw e;
            }

        }
        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockPost(Guid id)
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var postFromDb = await _repo.GetOneWithConditionTracking<Post>(post => post.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            if (User.IsInRole("superadmin") || User.IsInRole("forumadmin"))
            {
                postFromDb.IsLocked = postFromDb.IsLocked ? false : true;
            }
            else
            {
                // if (postFromDb.AccountId != accountId)
                // {
                //     return Unauthorized();
                // }
                postFromDb.IsLocked = postFromDb.IsLocked ? false : true;
            }
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            try
            {
                var postFromDb = await _repo.GetOneWithCondition<Post>(post => post.Id == id, "Comments, LikedPosts");
                if (postFromDb == null)
                {
                    return NotFound();
                }

                foreach (var likePost in postFromDb.LikedPosts)
                {
                    _repo.Delete(likePost);
                }
                var transactionPost = await _repo.GetOneWithCondition<Post>(p => p.Id == id);
                _repo.Delete(transactionPost);
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
                return StatusCode(500);
            }
            catch (System.Exception e)
            {

                throw e;
            }

        }
        [HttpDelete]
        public async Task<IActionResult> DeletePosts()
        {
            var posts = await _repo.GetAll<Post>(null, "");
            _repo.Delete(posts);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            return StatusCode(500);
        }
        private async Task<bool> ValidateString(string input)
        {
            var stringFilter = await _repo.GetAll<StringFilter>(null, "");
            int sensitiveWord = 0;
            if (stringFilter.Count() == 0)
            {
                return false;
            }
            var lowerInput = input.ToLower();
            foreach (var filter in stringFilter.OrderByDescending(f => f.Word.Length))
            {
                if (stringFilter.Any(f => f.Word.Equals(filter.Word) == false && f.Word.Contains(filter.Word)))
                {
                    continue;
                }
                else
                {
                    if (lowerInput.Contains(filter.Word.ToLower()))
                    {
                        sensitiveWord++;
                    }
                }
            }
            if (sensitiveWord >= Math.Round((double)stringFilter.Count() / 4))
            {
                return true;
            }
            return false;
        }
        private async Task<string> CensoredString(string input)
        {
            var stringFilter = await _repo.GetAll<StringFilter>(null, "");
            foreach (var filter in stringFilter.OrderByDescending(f => f.Word.Length))
            {
                var temp = "";
                foreach (char filterChar in filter.Word)
                {
                    if (!Char.IsWhiteSpace(filterChar))
                    {
                        temp += "*";
                    }
                    else
                    {
                        temp += " ";
                    }
                }
                input = input.Replace(filter.Word, temp);
            }
            return input;
        }
    }
}