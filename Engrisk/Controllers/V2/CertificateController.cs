using System;
using System.Threading.Tasks;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class CertificateController : BaseApiController
    {
        private readonly ICertificateService _certificateService;
        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            return Ok(await _certificateService.GetAllCertificatesAsync(pagination, search));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> CreateUserCertificate(int id, [FromBody] SignatureCertificateDTO signatureCertificateDTO)
        {
            return Ok(await _certificateService.CreateUserCertificateAsync(id, new Guid("3898A39A-5C53-4D71-A4B6-A8AABA5C2924"), signatureCertificateDTO));
        }
        [HttpGet("manage")]
        public async Task<IActionResult> GetManage([FromQuery] string search)
        {
            return Ok(await _certificateService.GetAllCertificatesWithoutPaginateAsync(search));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CertificateCreateDTO certificateCreate)
        {
            try
            {
                if (await _certificateService.CreateCertificateAsync(certificateCreate))
                {
                    return Ok();
                }
                return NoContent(); ;
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest(ex);
            }

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] CertificateCreateDTO certificateUpdate)
        {
            try
            {
                if (!await _certificateService.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if (await _certificateService.UpdateCertiifcateAsync(id, certificateUpdate))
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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _certificateService.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if (await _certificateService.DeleteCertificateAsync(id))
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest();
            }
        }
    }
}