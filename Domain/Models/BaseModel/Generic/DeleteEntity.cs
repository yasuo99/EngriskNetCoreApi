namespace Domain.Models.BaseModel.Generic
{
    public abstract class DeleteEntity<TKey>: EntityBase<TKey>, IDeleteEntity<TKey>
    {
        public bool IsDeleted { get; set; }
    }
}