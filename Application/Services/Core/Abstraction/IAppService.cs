using System.Threading.Tasks;
using Application.DTOs.App;

namespace Application.Services.Core.Abstraction
{
    public interface IAppService
    {
        Task<HomeDTO> GetHomeScreenDataAsync(int accountId);
    }
}