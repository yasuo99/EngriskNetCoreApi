using System;
using System.Threading.Tasks;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface ICertificateService
    {
        Task<bool> CheckExistAsync(Guid id);
        Task<bool> CheckExistAsync(string title);
        Task<PaginateDTO<CertificateDTO>> GetAllCertificatesAsync(PaginationDTO pagination, string search = null);
        Task<CertificateDTO> GetCertificateAsync(Guid id);
        Task<PaginateDTO<AccountCertificate>> GetUserCertificatesAsync(PaginationDTO pagination, int accountId, string search = null);
        Task<bool> CreateCertificateAsync(CertificateCreateDTO certificateCreateDTO);
        Task<string> CreateUserCertificateAsync(int accountId,SignatureCertificateDTO signatureCertificateDTO);
    }
}