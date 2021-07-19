using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Helper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Engrisk.Data;
using Application.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Engrisk.Controllers.V1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class PagesController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        public PagesController(ICRUDRepo repo)
        {
            _repo = repo;
        }
        [HttpGet("banners")]
        public async Task<IActionResult> GetAllBanner([FromQuery] SubjectParams subjectParams)
        {
            var banners = await _repo.GetAll<Banner>(subjectParams);
            return Ok(banners);
        }
        [HttpGet("banners/{id}")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var bannerFromDb = await _repo.GetOneWithCondition<Banner>(banner => banner.Id == id);
            if (bannerFromDb == null)
            {
                return NotFound();
            }
            return Ok(bannerFromDb);
        }
        [HttpGet("banners/published")]
        public async Task<IActionResult> GetPublishedBanner([FromQuery] SubjectParams subjectParams)
        {
            var banners = await _repo.GetAll<Banner>(subjectParams, banner => banner.IsPublished == true);
            return Ok(banners);
        }
        [HttpPost("banners")]
        public async Task<IActionResult> CreateBanner([FromForm] BannerDTO banner)
        {
            var file = banner.File;
            // var result = _cloudHelper.UploadImage(file);
            // if (result != null)
            // {
            //     var createdBanner = new Banner()
            //     {
            //         PublicId = result.PublicId,
            //         PhotoUrl = result.PublicUrl,
            //         IsPublished = false
            //     };
            //     _repo.Create(createdBanner);
            //     if (await _repo.SaveAll())
            //     {
            //         return Ok();
            //     }
            //     return StatusCode(500);
            // }
            return StatusCode(500);
        }
        [HttpDelete("banner/{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var bannerFromDb = await _repo.GetOneWithCondition<Banner>(banner => banner.Id == id);
            if (bannerFromDb == null)
            {
                return NotFound();
            }
            // if (_cloudHelper.DeleteImage(bannerFromDb.PublicId))
            // {
            //     _repo.Delete(bannerFromDb);
            //     if (await _repo.SaveAll())
            //     {
            //         return NoContent();
            //     }
            //     return StatusCode(500);
            // }
            return StatusCode(500);
        }
        [HttpPut("banners/{id}/publish")]
        public async Task<IActionResult> PublishBanner(int id)
        {
            var bannerFromDb = await _repo.GetOneWithCondition<Banner>(banner => banner.Id == id);
            if (bannerFromDb == null)
            {
                return NotFound();
            }
            bannerFromDb.IsPublished = true;
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpGet("footers")]
        public async Task<IActionResult> GetAllFooter([FromQuery] SubjectParams subjectParams)
        {
            var footersFromDb = await _repo.GetAll<Footer>(subjectParams);
            return Ok(footersFromDb);
        }
        [HttpGet("footers/{id}")]
        public async Task<IActionResult> GetFooter(int id){
            var footer = await _repo.GetOneWithCondition<Footer>(f => f.Id == id);
            if(footer == null){
                return NotFound();
            }
            return Ok(footer);
        }
        [HttpPost("foooters")]
        public async Task<IActionResult> CreateFooter([FromBody] Footer footer){
            return Ok();
        }
    }
}