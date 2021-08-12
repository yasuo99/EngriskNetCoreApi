using Domain.Enums;

namespace Domain.Models.BaseModel.Generic
{
    public abstract class PublishEntity : IPublishEntity
    {
        public PublishStatus PublishStatus { get; set; }
    }
}