using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Services
{
    public interface IUserService
    {
        Task<Account> UserResolveAsync();
    }
}