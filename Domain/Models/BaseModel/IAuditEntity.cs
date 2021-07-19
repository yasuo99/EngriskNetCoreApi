using System;

namespace Domain.Models.BaseModel
{
    public interface IAuditEntity
    {
         DateTime CreatedDate{get;set;}
         string CreatedBy{get;set;}
         DateTime? UpdatedDate{get;set;}
         string UpdatedBy{get;set;}
         
    }
}