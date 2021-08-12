using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Account.Route;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Application.Services.Core
{
    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        public CertificateService(ApplicationDbContext context, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<bool> CheckExistAsync(Guid id)
        {
            return await _context.Certificates.AnyAsync(cer => cer.Id == id);
        }

        public async Task<bool> CheckExistAsync(string title)
        {
            return await _context.Certificates.AnyAsync(cer => cer.Title.ToLower().Equals(title.ToLower()));
        }

        public async Task<bool> CreateCertificateAsync(CertificateCreateDTO certificateCreateDTO)
        {
            var certificate = _mapper.Map<Certificate>(certificateCreateDTO);
            if (certificateCreateDTO.Template != null)
            {
                certificate.Template = _fileService.UploadFile(certificateCreateDTO.Template, SD.CertificatePath);
            }
            _context.Certificates.Add(certificate);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<string> CreateUserCertificateAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateUserCertificateAsync(int accountId, Guid certificateId, SignatureCertificateDTO signatureCertificateDTO)
        {
            string path = Path.Combine(_fileService.ContentRootPath, Path.Combine(SD.FontPath, "Great_Vibes\\GreatVibes-Regular.ttf"));
            string path2 = Path.Combine(_fileService.ContentRootPath, Path.Combine(SD.FontPath, "Dancing_Script\\DancingScript-VariableFont_wght.ttf"));
            //Read font file             
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(path);
            pfc.AddFontFile(path2);
            //Instantiate font             
            //Set font            
            var certificate = await _context.Certificates.AsNoTracking().FirstOrDefaultAsync(certi => certi.Id == certificateId);
            string title = certificate.Subject;
            string detail = $"{certificate.Title}";
            string score = $"{signatureCertificateDTO.Score}";
            var date = DateTime.Now;
            var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(acc => acc.Id == accountId);
            string sign = String.IsNullOrEmpty(signatureCertificateDTO.Signature) ? account.UserName : signatureCertificateDTO.Signature;

            string imageFilePath = Path.Combine(_fileService.ContentRootPath, Path.Combine(SD.CertificatePath, "template.png"));

            Bitmap newBitmap;
            using (var bitmap = (Bitmap)Image.FromFile(imageFilePath))//load the image file
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    using (Font arialFont = new Font(pfc.Families[1], 50))
                    {

                        using (var sf = new StringFormat()
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                        })
                        {
                            graphics.DrawString(title, arialFont, Brushes.Black, new Rectangle(0, 0, bitmap.Width, bitmap.Height), sf);
                            using (Font temp = new Font(pfc.Families[0], 20))
                            {
                                graphics.DrawString(detail, temp, Brushes.Black, new Rectangle(0, 100, bitmap.Width, bitmap.Height), sf);
                            }
                            using (Font temp = new Font(pfc.Families[0], 40))
                            {
                                graphics.DrawString(score, temp, Brushes.Blue, new Rectangle(0, 180, bitmap.Width, bitmap.Height), sf);
                            }
                            using (Font temp = new Font(pfc.Families[0], 30))
                            {
                                graphics.DrawString(sign, temp, Brushes.Black, new Rectangle(420, 430, bitmap.Width, bitmap.Height), sf);
                                graphics.DrawString(date.Date.ToString("dd/MM/yyyy"), temp, Brushes.Black, new Rectangle(-400, 430, bitmap.Width, bitmap.Height), sf);
                            }

                        }

                        //graphics.DrawString(firstText, arialFont, Brushes.Black, firstLocation);
                        //graphics.DrawString(secondText, arialFont, Brushes.Black, secondLocation);

                    }
                }
                newBitmap = new Bitmap(bitmap);
            }
            var filePath = Path.Combine(_fileService.ContentRootPath, SD.CertificatePath);
            string fileName = $"{account.UserName}-{date.Date.ToString("dd-MM-yyyy")}.png";
            var result = Path.Combine(filePath, fileName);
            newBitmap.Save(result);//save the image file
            newBitmap.Dispose();
            var returnPath = _fileService.GetAppBaseUrl(Path.Combine(SD.CertificatePath, fileName), "image");
            return returnPath;
        }

        public async Task<bool> DeleteCertificateAsync(Guid id)
        {
            var certificate = await _context.Certificates.FirstOrDefaultAsync(cer => cer.Id == id);
            _context.Remove(certificate);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<PaginateDTO<CertificateDTO>> GetAllCertificatesAsync(PaginationDTO pagination, string search = null)
        {
            var certificates = from c in _context.Certificates.OrderByDescending(orderBy => orderBy.UpdatedDate).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking() select c;
            if (search != null)
            {
                certificates = certificates.Where(c => c.Title.ToLower().Contains(search.Trim().ToLower()));
            }


            var pagingListCertificatesDto = await PagingList<Certificate>.OnCreateAsync(certificates, pagination.CurrentPage, pagination.PageSize);
            var result = pagingListCertificatesDto.CreatePaginate();
            var certificatesDto = _mapper.Map<List<CertificateDTO>>(result.Items);
            foreach (var certificate in certificatesDto)
            {
                certificate.Route = _mapper.Map<RouteDTO>(await _context.Routes.Where(route => route.Sections.Any(sec => sec.Scripts.Any(script => script.Certificate.Id == certificate.Id))).AsNoTracking().FirstOrDefaultAsync());
            }

            return new PaginateDTO<CertificateDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Items = certificatesDto,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        public async Task<List<CertificateDTO>> GetAllCertificatesWithoutPaginateAsync(string search = null)
        {
            var certificates = await _context.Certificates.Where(cer => cer.ScriptId == Guid.Empty || cer.ScriptId == null).AsNoTracking().ToListAsync();
            if (search != null)
            {
                certificates = certificates.Where(c => c.Title.ToLower().Contains(search.Trim().ToLower())).ToList();
            }
            var certificatesDto = _mapper.Map<List<CertificateDTO>>(certificates);
            return certificatesDto;
        }

        public async Task<CertificateDTO> GetCertificateAsync(Guid id)
        {
            var certificate = await _context.Certificates.Where(cer => cer.Id == id).AsNoTracking().FirstOrDefaultAsync();
            var certificateDto = _mapper.Map<CertificateDTO>(certificate);
            certificateDto.Route = _mapper.Map<RouteDTO>(await _context.Routes.Where(route => route.Sections.Any(sec => sec.Scripts.Any(script => script.Certificate.Id == id))).FirstOrDefaultAsync());
            return certificateDto;
        }

        public async Task<PaginateDTO<AccountCertificate>> GetUserCertificatesAsync(PaginationDTO pagination, int accountId, string search = null)
        {
            var certificates = await _context.AccountCertificates.Where(ac => ac.AccountId == accountId).Include(inc => inc.Certificate).AsNoTracking().ToListAsync();
            if (search != null)
            {
                certificates = certificates.Where(cer => cer.Certificate.Title.ToLower().Contains(search.Trim().ToLower()) || cer.Certificate.Subject.ToLower().Contains(search.Trim().ToLower())).ToList();
            }
            var pagingListCertificates = PagingList<AccountCertificate>.OnCreate(certificates, pagination.CurrentPage, pagination.PageSize);
            return pagingListCertificates.CreatePaginate();
        }

        public async Task<bool> UpdateCertiifcateAsync(Guid id, CertificateCreateDTO certificateCreateDTO)
        {
            var certificate = await _context.Certificates.FirstOrDefaultAsync(cer => cer.Id == id);
            _mapper.Map(certificateCreateDTO, certificate);
            if (certificateCreateDTO.Template != null)
            {
                _fileService.DeleteFile(certificate.Template);
                certificate.Template = _fileService.UploadFile(certificateCreateDTO.Template, SD.CertificatePath);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
    }
}