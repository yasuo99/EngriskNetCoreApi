using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Exam;
using Application.DTOs.Pagination;
using Application.DTOs.Ticket.VerifyTicket;
using Application.Mediator.Accounts;
using Application.Services.Core;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class AccountsController : BaseApiController
    {
        private readonly IAccountService _service;
        public AccountsController(IAccountService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<List<Account>>> GetAll()
        {
            return Ok(await Mediator.Send(new List.Query()));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var account = await Mediator.Send(new Detail.Query { Id = id });
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRoles(int id)
        {
            return Ok(await _service.GetAccountRolesAsync(id));
        }
        [HttpGet("{id}/certificates")]
        public async Task<IActionResult> GetUserCertificates(int id, [FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            // if(Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) != id){
            //     return Unauthorized();
            // }
            return Ok(await _service.GetUserCertificatesAsync(pagination, id, search));
        }
        [HttpGet("{id}/questions")]
        public async Task<IActionResult> GetUserQuestions(int id, [FromQuery] PaginationDTO pagination, [FromQuery] string search, [FromQuery] QuestionType type)
        {
            // if(Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) != id){
            //     return Unauthorized();
            // }
            return Ok(await _service.GetUserQuestionAsync(pagination, type, id, search));
        }
        [Authorize]
        [HttpPut("{followerId}/following/{followingId}")]
        public async Task<IActionResult> FollowUser(int followerId, int followingId)
        {
            if (followerId == followingId)
            {
                return BadRequest();
            }
            if (followerId != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var follower = await _service.GetAccountAsync(followerId);
            if (follower == null)
            {
                return NotFound(new
                {
                    Error = "Follower didn't exist"
                });
            }
            var following = await _service.GetAccountAsync(followingId);
            if (following == null)
            {
                return NotFound(new
                {
                    Error = "Following didn't exist"
                });
            }
            var followStatus = await _service.FollowingUserAsync(follower, following);
            if (followStatus == true)
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(int accountId)
        {
            return Ok(await _service.GetAccountFollowing(accountId));
        }
        [HttpPut("{followerId}/following/{followingId}/notification")]
        public async Task<IActionResult> SwitchFollowNotification(int followerId, int followingId)
        {
            var follower = await _service.GetAccountAsync(followerId);
            var following = await _service.GetAccountAsync(followingId);
            await _service.SwitchNotificationAsync(follower, following);
            return Ok();
        }
        [Authorize]
        [HttpPost("{id}/boxchat")]
        public async Task<IActionResult> CreatePrivateBoxChat(int id, [FromBody] BoxChat boxChat)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            return Ok();
        }
        [Authorize]
        [HttpGet("{id}/boxchat/{boxchatId}")]
        public async Task<IActionResult> SendInvite(int id, Guid boxchatId, [FromQuery] int receiverId)
        {
            if (!await _service.CheckBoxchatOwnerAsync(boxchatId, id))
            {
                return BadRequest();
            }
            await _service.InviteUserToBoxchat(receiverId, boxchatId);
            return Ok();
        }
        [Authorize]
        [HttpGet("{userId}/groups")]
        public async Task<IActionResult> GetUserGroup(int userId)
        {
            return Ok(await _service.GetAllUserGroupAsync(userId));
        }
        [Authorize]
        [HttpPut("{userId}/groups/{groupId}/words/{wordId}")]
        public async Task<IActionResult> AddWordToGroup(int userId, Guid groupId, Guid wordId)
        {
            var group = await _service.GetGroupAsync(groupId);
            if (group == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy group"
                });
            }
            var word = await _service.GetWordAsync(wordId);
            if (word == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy từ vựng"
                });
            }
            await _service.WordGroupActionAsync(group, word);
            return Ok();
        }
        [Authorize]
        [HttpGet("{id}/vocabulary/learned")]
        public async Task<IActionResult> GetLearnedVocabulary(int id)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetWordLearntPathAsync(id));
        }
        [Authorize]
        [HttpGet("{id}/vocabulary/review")]
        public async Task<IActionResult> GetVocabularyReviewQuestion(int id, [FromQuery] string option)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            return Ok(await _service.VocabularyReviewAsync(id, option));
        }
        //Resources
        [Authorize]
        [HttpGet("{id}/resources/shared/exams")]
        public async Task<IActionResult> GetSharedExamResources(int id, [FromQuery] PaginationDTO pagination)
        {
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetSharedExamAsync(id, pagination));
        }
        [Authorize]
        [HttpGet("{id}/resources/shared/quizzes")]
        public async Task<IActionResult> GetSharedQuizzesResources(int id, [FromQuery] PaginationDTO pagination)
        {
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetSharedQuizAsync(id, pagination));
        }
        [Authorize]
        [HttpGet("{id}/resources/quizzes/{quizId}/sharing")]
        public async Task<IActionResult> GetSharingDetail(int id, Guid quizId)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetQuizSharingInformationAsync(id, quizId));
        }
        [Authorize]
        [HttpPut("{id}/resources/quizzes/{quizId}/sharing")]
        public async Task<IActionResult> SharingQuiz(int id, Guid quizId, [FromBody] List<int> users)
        {
            var result = await _service.CheckAccounts(users);
            if (result > -1)
            {
                return BadRequest(new
                {
                    Error = $"Tài khoản {result} không tồn tại"
                });
            }
            var sharedResult = await _service.SharingQuizAsync(id, quizId, users);
            if (sharedResult)
            {
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpDelete("{id}/resourses/quizzes/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int id, Guid quizId)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            await _service.DeleteQuizAsync(id, quizId);
            return NoContent();
        }
        [Authorize]
        [HttpGet("{id}/resources/exams/{examId}/sharing")]
        public async Task<IActionResult> GetExamSharingDetail(int id, Guid examId)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetExamSharingInformationAsync(id, examId));
        }
        [Authorize]
        [HttpPut("{id}/resources/exams/{examId}/sharing")]
        public async Task<IActionResult> SharingExam(int id, Guid examId, [FromBody] List<int> users)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var sharedResult = await _service.SharingExamAsync(id, examId, users);
            if (sharedResult)
            {
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpDelete("{id}/resources/exams/{examId}")]
        public async Task<IActionResult> DeleteExam(int id, Guid examId)
        {
            if (id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            await _service.DeleteExamAsync(id, examId);
            return NoContent();
        }
        //Boxchat
        [Authorize]
        [HttpPost("{id}/boxchats")]
        public async Task<IActionResult> CreateBoxchat(int id, [FromBody] BoxchatCreateDTO boxchatCreateDTO)
        {
            try
            {
                if (Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) != id)
                {
                    return Unauthorized();
                }
                var boxchat = await _service.CreateBoxChatAsync(id, boxchatCreateDTO);
                if (boxchat != null)
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest(ex);
            }
        }
        [Authorize]
        [HttpGet("{id}/boxchats")]
        public async Task<IActionResult> GetUserBoxChat(int id)
        {
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            return Ok(await _service.GetUserBoxchatAsync(id));
        }
        [Authorize]
        [HttpGet("{id}/boxchats/{boxchatId}")]
        public async Task<IActionResult> GetBoxchatDetail(int id, Guid boxchatId)
        {
            // if(id != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
            //     return Unauthorized();
            // }
            return Ok(await _service.GetBoxchatMessageAsync(id, boxchatId));
        }
        [Authorize]
        [HttpPut("{id}/boxchats/{boxchatId}")]
        public async Task<IActionResult> UpdateBoxchat(int id, Guid boxchatId, [FromBody] BoxchatUpdateDTO boxchat){
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            if(!await _service.CheckBoxchatOwnerAsync(boxchatId,id)){
                return BadRequest();
            }
            if(await _service.UpdateBoxchatAsync(boxchatId,boxchat)){
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}/boxchats/{boxchatId}/members")]
        public async Task<IActionResult> InviteUsersToBoxchat(int id, Guid boxchatId, [FromBody] List<int> users)
        {
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            if (!await _service.CheckBoxchatOwnerAsync(boxchatId, id))
            {
                return BadRequest();
            }
            await _service.InviteUsersToBoxchatAsync(boxchatId, users);
            return Ok();
        }
        [Authorize]
        [HttpDelete("{id}/boxchats/{boxchatId}")]
        public async Task<IActionResult> DeleteBoxchat(int id, Guid boxchatId){
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            if(!await _service.CheckBoxchatOwnerAsync(boxchatId,id)){
                return BadRequest();
            }
            if(await _service.DeleteBoxchatAsync(boxchatId)){
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}/notifications/{notificationId}/invite")]
        public async Task<IActionResult> ResponseToInvite(int id, Guid notificationId, [FromQuery] string action)
        {
            if (id != Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Unauthorized();
            }
            if (!await _service.CheckBoxchatInviteAsync(notificationId, id))
            {
                return BadRequest();
            }
            await _service.AcceptBoxChatInvite(id, notificationId, action);
            return Ok();
        }
        [Authorize]
        [HttpGet("{id}/learning/history")]
        public async Task<IActionResult> GetLearningHistory(int id, [FromQuery] DateRangeDTO dateRange)
        {
            if (Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) != id)
            {
                return Unauthorized();
            }
            return Ok(await _service.GetDailyLearningAsync(id, dateRange));
        }
        [Authorize]
        [HttpPut("{id}/routes/{routeId}/select")]
        public async Task<IActionResult> SelectRoute(int id, Guid routeId)
        {
            if (Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) != id)
            {
                return Unauthorized();
            }
            if (await _service.SelectRouteAsync(id, routeId))
            {
                return Ok();
            }
            return NoContent();
        }
        //Data
        [HttpGet("{id}/data/{routeId}")]
        public async Task<IActionResult> MakeRouteFinish(int id, Guid routeId){
            return Ok( await _service.MakeUserFinishRouteAsync(id,routeId));
        }
    }
}