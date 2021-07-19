namespace Domain.Models.BaseModel.Generic
{
    public interface IAuditEntity<TKey>: IAuditEntity, IEntityBase<TKey>
    {
         
    }
}