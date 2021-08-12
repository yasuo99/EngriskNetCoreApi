using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.Hubs;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Engrisk.Controllers.V2
{
    public class PostsController : BaseApiController
    {
        private IHubContext<NotificationHub> _hub;
        private IPostService _postService;
        public PostsController(IHubContext<NotificationHub> hub, IPostService postService)
        {
            _hub = hub;
            _postService = postService;
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PostTypes type)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                return Ok(await _postService.GetAllPosts(type, accountId));
            }
            return Ok(await _postService.GetAllPosts(type));
        }
        [HttpGet("manage")]
        public async Task<IActionResult> GetManage([FromQuery] PaginationDTO pagination, [FromQuery] string search){
            return Ok(await _postService.GetAllPosts(pagination,search: search));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> VerifyContent(Guid id, [FromQuery] Status status)
        {
            return Ok(await _postService.CensorContentAsync(id, status));
        }
        [Authorize]
        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockPost(Guid id){
            try
            {
                if(!await _postService.CheckExistAsync(id)){
                    return NotFound();
                }
                if(await _postService.LockPostAsync(id)){
                    return Ok();
                }
                return NoContent();
            }   
            catch (System.Exception ex)
            {
                return BadRequest(ex);
                 // TODO
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // var post = await Repo.Post.FindOneAsync(p => p.Id == id);
            // Repo.Post.Delete(post);
            // if(await Repo.SaveAsync()){
            //     return Ok();
            // }
            return NoContent();
        }
    }
}