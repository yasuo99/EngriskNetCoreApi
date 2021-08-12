using System;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Services.Core.Abstraction
{
    public interface IPublishService
    {
         Task PublishAsync(Guid id, PublishStatus status);
    }
}