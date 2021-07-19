using System.Threading.Tasks;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class CertificateController: BaseApiController
    {
        private readonly ICertificateService _certificateService;
        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination, [FromQuery] string search){
            return Ok(await _certificateService.GetAllCertificatesAsync(pagination,search));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> CreateUserCertificate(int id, [FromBody] SignatureCertificateDTO signatureCertificateDTO){
            return Ok(await _certificateService.CreateUserCertificateAsync(id, signatureCertificateDTO));
        }
    }
}