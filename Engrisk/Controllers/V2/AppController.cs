using System.Threading.Tasks;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class AppController : BaseApiController
    {
        private readonly IAppService _appService;
        public AppController(IAppService appService)
        {
            _appService = appService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHomeScreenData(int id)
        {
            return Ok(await _appService.GetHomeScreenDataAsync(id));
        }
    }
}