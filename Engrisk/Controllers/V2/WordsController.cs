using System;
using System.Threading.Tasks;
using AutoMapper;
using Application.DTOs.Word;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Helper;
using Application.DTOs.Pagination;
using Application.DTOs.Memory;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;
using System.Collections.Generic;

namespace Engrisk.Controllers.V2
{
    public class WordsController : BaseApiController
    {
        private readonly IWordService _wordService;
        private readonly UserManager<Account> _userManager;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IMapper _mapper;
        public WordsController(IWordService wordService, UserManager<Account> userManager, IHubContext<NotificationHub> hub)
        {
            _wordService = wordService;
            _userManager = userManager;
            _hub = hub;
        }
        [HttpGet("tospeech")]
        public async Task<IActionResult> Amen()
        {
            return Ok("wtf");
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search = null)
        {
            var words = await _wordService.GetAllAsync(pagination, search);
            return Ok(words);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWord(Guid id)
        {
            var word = await _wordService.GetDetailWithMemoriesAsync(id);
            if (word == null)
            {
                return NotFound();
            }
            return Ok(word);
        }
        [HttpGet("{id}/questions")]
        public async Task<IActionResult> GetVocabularyPracticeQuestion(Guid id){
            if(!await _wordService.CheckExistAsync(id)){
                return NotFound();
            }
            return Ok(await _wordService.GetVocabularyPracticeQuestionsAsync(id));
        }
        [HttpGet("inserted")]
        public async Task<IActionResult> GetVocabularyForCreateScript([FromQuery] string search){
            return Ok(await _wordService.GetVocabularyForScriptAsync(search));
        }
        /// <summary>
        /// Add new word to system
        /// </summary>
        /// <param name="wordDto">Object to create word</param>
        /// <returns>Word has been created</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWord([FromForm] WordCreateDTO wordDto)
        {
            if (await _wordService.CheckConflictAsync(wordDto))
            {
                return Conflict(new
                {
                    Status = 409,
                    Type = "Create",
                    Error = "Trùng từ vựng"
                });
            }
            var wordCreated = await _wordService.CreateWordAsync(wordDto);
            if (wordCreated != null)
            {
                var responseWord = Extension.CamelCaseSerialize(wordCreated);
                foreach (var client in HubHelper.NotificationClientsConnections)
                {
                    await _hub.Clients.Client(client.ClientId).SendAsync("AddWord", responseWord);
                }
                return Ok(new
                {
                    Status = 200,
                    Type = "Create",
                    Result = wordCreated
                });
            };
            return NoContent();
        }
        /// <summary>
        /// Update exist word in system
        /// </summary>
        /// <param name="id">Identity of word</param>
        /// <param name="wordUpdateDTO">Object contain data need to udpate</param>
        /// <returns>Word has been updated</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWord(Guid id, [FromForm] WordUpdateDTO wordUpdateDTO)
        {
            if(!await _wordService.CheckExistAsync(id)){
                return NotFound();
            }
            return Ok(await _wordService.UpdateAsync(id, wordUpdateDTO));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWord(Guid id)
        {
            var word = await _wordService.GetDetailAsync(id);
            if (word == null)
            {
                return NotFound();
            }
            await _wordService.DeleteWordAsync(word);
            return NoContent();
        }
        /// <summary>
        /// Create new memory for word
        /// </summary>
        /// <param name="id">Identify of word</param>
        /// <param name="memoryCreateDTO">Memory object to create</param>
        /// <returns>Memory has created</returns>
        [Authorize]
        [HttpPost("{id}/memories")]
        public async Task<IActionResult> CreateWordMemory(Guid id, [FromForm] MemoryCreateDTO memoryCreateDTO)
        {
            if (id != memoryCreateDTO.WordId)
            {
                return BadRequest();
            }
            var word = await _wordService.GetDetailWithMemoriesAsync(id);
            if (word == null)
            {
                return NotFound();
            }
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var memory = await _wordService.CreateWordMemoryAsync(word, memoryCreateDTO, accountId);
            if (memory != null)
            {
                return Ok(memory);
            }
            return NoContent();
        }
        [Authorize]
        [HttpDelete("{wordId}/memories/{memoryId}")]
        public async Task<IActionResult> DeleteMemory(Guid wordId, Guid memoryId)
        {
            try
            {
                var word = await _wordService.GetDetailAsync(wordId);
                if (word == null)
                {
                    return NotFound();
                }
                var memory = await _wordService.GetMemoryAsync(word, memoryId);
                if (memory == null)
                {
                    return BadRequest();
                }
                await _wordService.DeleteMemoryAsync(memory);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // TODO
                return StatusCode(500);
            }

        }
        [Authorize]
        [HttpPut("{wordId}/memories/{memoryId}")]
        public async Task<IActionResult> SetMemory(Guid wordId, Guid memoryId)
        {
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var word = await _wordService.GetDetailWithMemoriesAsync(wordId);
            if (word == null)
            {
                return NotFound();
            }
            var memory = await _wordService.GetMemoryAsync(word, memoryId);
            if (memory == null)
            {
                return NotFound();
            }
            await _wordService.SelectMemoryAsync(accountId, wordId, memoryId);
            return Ok();
        }
        [HttpPost("{wordId}/examples/contribute")]
        public async Task<IActionResult> ContributeExample(Guid wordId, [FromBody] Example example)
        {
            var word = await _wordService.GetDetailAsync(wordId);
            if (word == null)
            {
                return NotFound();
            }
            return Ok(await _wordService.ContributeExampleAsync(wordId, example));
        }
        [HttpPost("{wordId}/questions")]
        public async Task<IActionResult> AddWordPracticeQuestion(Guid wordId, [FromForm] WordQuestionsCreateDTO wordQuestionsCreateDTO)
        {
            return Ok();
        }
        [HttpPut("{wordId}/questions")]
        public async Task<IActionResult> UpdateWordPracticeQuestions(Guid wordId, [FromForm] WordQuestionsCreateDTO wordQuestionsCreateDTO)
        {
            return Ok();
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchWord([FromQuery] string word = "")
        {
            if (word != null)
            {
                var result = await _wordService.SearchWordAsync(word);
                return Ok(result);
            }
            return NoContent();
        }
        [HttpGet("translate")]
        public async Task<IActionResult> Translate([FromQuery] string content = "")
        {
            if (content != null)
            {
                return Ok(await _wordService.TranslateAsync(content));
            }
            return NoContent();
        }
        [Authorize]
        [AllowAnonymous]
        [HttpPost("review")]
        public async Task<IActionResult> VocabularyReview([FromBody] List<Guid> words){
            return Ok(await _wordService.VocabularyReviewAsync(words));
        }
        // [Authorize]
        // [HttpPost("contribute")]
        // public async Task<IActionResult> ContributorWord([FromForm] WordCreateDTO wordCreateDTO)
        // {
        //     return Ok();
        // }
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateWord(Guid id, [FromForm] WordUpdateDTO wordUpdateDto)
        // {
        //     try
        //     {
        //         if (!await Repo.Word.ExistAsync(id))
        //         {
        //             return NotFound();
        //         }
        //         await Repo.Word.UpdateWordAsync(id,wordUpdateDto);
        //     }
        //     catch (System.Exception e)
        //     {
        //         return BadRequest(new
        //         {
        //             Error = e.Message
        //         });
        //     }
        //     return Ok();
        // }
        // [HttpPut("{id}/examples")]
        // public async Task<IActionResult> ContributorWordExamples(Guid id)
        // {
        //     return Ok();
        // }
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteWord(Guid id)
        // {
        //     try
        //     {
        //         if (!await Repo.Word.ExistAsync(id))
        //         {
        //             return NotFound();
        //         }
        //         await Repo.Word.DeleteAsync(id);
        //         if (await Repo.SaveAsync())
        //         {
        //             return Ok();
        //         }
        //         return NoContent();
        //     }
        //     catch (System.Exception e)
        //     {
        //         return BadRequest(new
        //         {
        //             Error = e.Message
        //         });
        //     }
        // }
    }
}