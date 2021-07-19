using System;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Services.Core.Abstraction
{
    public interface ICensorService
    {
         Task<bool> CensorContentAsync(Guid id, Status status);
    }
}