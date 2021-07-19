using System.IO;
using System.Threading.Tasks;
using Application.Services;
using Engrisk.Data;
using Engrisk.Services;
using Engrisk.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    [ApiController]
    [Route("api/v2/[Controller]")]
    public class AudioController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IFileService _fileService;
        public AudioController(ICRUDRepo repo, IFileService fileService)
        {
            _fileService = fileService;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> StreamAudio([FromQuery] string filename)
        {
            var filePath = Path.Combine(_fileService.ContentRootPath, SD.AudioPath);
            return PhysicalFile(Path.Combine(filePath,filename), "application/octet-stream", enableRangeProcessing: true, fileDownloadName: filename);
        }
    }
}