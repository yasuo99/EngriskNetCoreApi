using Domain.Enums;

namespace Domain.Models.BaseModel
{
    public interface IPublishEntity
    {
        PublishStatus PublishStatus { get; set; }
    }
}