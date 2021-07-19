using System;
using System.IO;
using System.Threading.Tasks;
using Application.Services;
using Engrisk.Data;
using Engrisk.Services;
using Engrisk.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    [ApiController]
    [Route("api/v2/[Controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IFileService _fileService;
        public FilesController(ICRUDRepo repo, IFileService fileService)
        {
            _fileService = fileService;
            _repo = repo;
        }
        [HttpGet("audio")]
        public FileResult AudioStream(string filePath)
        {
            return PhysicalFile(filePath, "application/octet-stream", enableRangeProcessing: true);
        }
        [HttpGet("image")]
        public FileResult ImageStream(string filePath)
        {
            return PhysicalFile(filePath, "image/*");
        }
        [HttpGet("file")]
        public async Task<IActionResult> GetFile([FromQuery] string filename){
            string filePath = Path.Combine(_fileService.ContentRootPath, SD.ImagePath);
            Stream stream = new FileStream(Path.Combine(filePath,filename), FileMode.Open, FileAccess.Read);
            return File(stream, "application/octet-stream", fileDownloadName: filename);
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            _fileService.UploadFile(file, SD.ImagePath);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string filename){
            string filePath = Path.Combine(_fileService.ContentRootPath, SD.ImagePath);
            System.IO.File.Delete(Path.Combine(filePath,filename));
            return Ok();
        }
        private string UploadAudio(IFormFile file)
        {
            throw new NotImplementedException();
        }

        private string UploadImage(IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
    public class Test {
        public FileResult File { get; set; }
    }
}