using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Application.Helper;
using Application.Services;
using Application.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class StreamingController : BaseApiController
    {
        private IFileService _fileService;
        public StreamingController([NotNull] IFileService fileService)
        {
            _fileService = fileService;
        }
        [HttpGet("audio")]
        public async Task<IActionResult> StreamAudio([FromQuery] string audio)
        {
            var filePath = Path.Combine(_fileService.ContentRootPath, audio);
            if (System.IO.File.Exists(filePath))
            {
                return PhysicalFile(filePath, "application/octet-stream", enableRangeProcessing: true);
            }
            return NotFound();
        }
        [HttpGet("image")]
        public async Task<IActionResult> StreamImage([FromQuery] string image)
        {
            var filePath = Path.Combine(_fileService.ContentRootPath, image);
            if (System.IO.File.Exists(filePath))
            {
                var tempImage = System.IO.File.ReadAllBytes(filePath);
                return File(tempImage, "image/*", _fileService.GetImageFileName(image));
            }
            return NotFound();

        }
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            var filePath = _fileService.UploadFile(upload, SD.ImagePath);
            Extension.TempImagePath.Add(filePath);
            return Ok(new
            {
                fileName = upload.FileName,
                uploaded = 1,
                url = filePath
            });
        }
    }
}