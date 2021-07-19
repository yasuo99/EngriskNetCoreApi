namespace Domain.Models.BaseModel.Generic
{
    public interface IEntityBase<TKey>
    {
         TKey Id { get; set; }
    }
}