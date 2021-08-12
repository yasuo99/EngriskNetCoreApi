using System;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.Services.Core.Abstraction;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class CategoryTagController : BaseApiController
    {
        private readonly ICategoryTagService _tagService;
        public CategoryTagController(ICategoryTagService tagService)
        {
            _tagService = tagService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            return Ok(await _tagService.GetAllCategoryTagAsync(pagination, search));
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _tagService.GetAllCategoryTagAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryTag tag)
        {
            try
            {
                if (await _tagService.CreateCategoryTagAsync(tag))
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryTag tag)
        {
            try
            {
                if (!await _tagService.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if (await _tagService.UpdateCategoryTagAsync(id, tag))
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id){
            try
            {
                if (!await _tagService.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if(await _tagService.DeleteCategoryTagAsync(id)){
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
    }
}