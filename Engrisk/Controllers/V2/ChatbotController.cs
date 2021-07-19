using System.Threading.Tasks;
using Application.DTOs;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class ChatbotController : BaseApiController
    {
        private readonly IWordService _wordService;
        public ChatbotController(IWordService wordService)
        {
            _wordService = wordService;
        }
        [HttpGet("dictionary")]
        public async Task<IActionResult> DictionaryApi([FromQuery] string vocabulary){
            var result = await _wordService.SearchWordAsync(vocabulary);
            if(result != null){
                return Ok(result);
            }
            return NoContent();
        }
        [HttpPost("translate")]
        public async Task<IActionResult> TranslateApi([FromBody] TranslationDTO translation){
            return Ok(await _wordService.TranslateAsync(translation.Text));
        }
    }
}