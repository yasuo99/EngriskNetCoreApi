using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class CategoryTagController: BaseApiController
    {   
        private readonly ICategoryTagService _tagService;
        public CategoryTagController(ICategoryTagService tagService)
        {
            _tagService = tagService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search){
            return Ok(await _tagService.GetAllCategoryTagAsync(pagination,search));
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(){
            return Ok(await _tagService.GetAllCategoryTagAsync());
        }
    }
}