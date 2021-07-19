using System;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Word.WordCategory;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class WordCategoriesController : BaseApiController
    {
        private readonly IWordCategoryService _wordCateService;
        public WordCategoriesController(IWordCategoryService wordCateService)
        {
            _wordCateService = wordCateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search, [FromQuery] bool learn, [FromQuery] string tag)
        {
            return Ok(await _wordCateService.GetAllAsync(pagination: pagination, search, learn, tag));
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllWithoutPagination()
        {
            return Ok(await _wordCateService.GetAllAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var wordCategory = await _wordCateService.GetDetailAsync(id);
            if (wordCategory == null)
            {
                return NotFound();
            }
            return Ok(wordCategory);
        }
        [HttpGet("{id}/users/{userId}")]
        public async Task<IActionResult> GetDetailWithUser(Guid id, int userId)
        {
            var wordCategory = await _wordCateService.UserGetDetailAsync(userId, id);
            if (wordCategory == null)
            {
                return NotFound();
            }
            return Ok(wordCategory);
        }
        // [HttpGet("{id}/practice")]
        // public async Task<IActionResult> Practice(Guid id)
        // {
        //     if (!await Repo.WordCategory.ExistAsync(id))
        //     {
        //         return NotFound();
        //     }
        //     var questions = await Repo.WordCategory.PracticeAsync(id);
        //     return Ok(questions);
        // }
        [HttpGet("{id}/practice")]
        public async Task<IActionResult> WordPractice(Guid id)
        {
            var questions = await _wordCateService.GetPracticeQuestion(id);
            if (questions == null)
            {
                return NoContent();
            }
            return Ok(questions);
        }
        [HttpPost]
        public async Task<IActionResult> CreateWordCategory([FromForm] WordCategoryCreateDTO wordCategoryCreateDTO)
        {
            if (await _wordCateService.CheckConflictAsync(wordCategoryCreateDTO.CategoryName))
            {
                return Conflict(new
                {
                    Error = "Trùng nhóm từ vựng"
                });
            }
            var wordCategory = await _wordCateService.CreateCategoryAsync(wordCategoryCreateDTO);
            if (wordCategory != null)
            {
                return Ok(new{
                    status = 200,
                    data = wordCategory
                });
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWordCategory(Guid id, [FromForm] WordCategoryCreateDTO wordCategoryUpdateDTO)
        {
            if(!await _wordCateService.ExistAsync(id)){
                return NotFound();
            }
            var updatedWordCategory = await _wordCateService.UpdateCategoryAsync(id, wordCategoryUpdateDTO);
            if(updatedWordCategory != null){
                return Ok(new{
                    status = 200,
                    data = updatedWordCategory
                });
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWordCategory(Guid id)
        {
            if(!await _wordCateService.ExistAsync(id)){
                return NotFound();
            }
            await _wordCateService.DeleteCategoryAsync(id);
            return NoContent();
        }

    }
}