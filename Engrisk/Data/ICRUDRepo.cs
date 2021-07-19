using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Helper;

namespace Engrisk.Data
{
    public interface ICRUDRepo
    {
         Task<PagingList<T>> GetAll<T>(SubjectParams subjectParams = null,Expression<Func<T, bool>> expression = null, string includeProperties = "", 
         Func<IQueryable<T>,IOrderedQueryable<T>> orderBy = null) where T:class;
         Task<List<T>> GetAll<T>(Expression<Func<T,bool>> expression, string includeProperties = "",
         Func<IQueryable<T>,IOrderedQueryable<T>> orderBy = null) where T:class;
         Task<T> GetOneWithConditionTracking<T>(Expression<Func<T,bool>> expression = null, string includeProperties = "") where T:class;
         Task<IQueryable<T>> GetOneWithManyToMany<T>(Expression<Func<T,bool>> expression=null) where T:class;
         Task<T> GetOneWithCondition<T>(Expression<Func<T,bool>> expression = null, string includeProperties = "") where T: class;
         Task<T> GetOneWithKey<T>(dynamic key) where T:class;
         IEnumerable<T> GetWithProperties<T>(Dictionary<dynamic,dynamic> properties, Expression<Func<T,bool>> expression = null, string includeProperties = "") where T:class;
         void Create<T>(T subject) where T:class;
         void Update<T>(T subject) where T:class;
         void Delete<T>(dynamic id) where T:class;
         void Delete<T>(T subject) where T:class;
         void Delete<T>(IEnumerable<T> subjects) where T:class;
         void Delete<T>() where T:class;
         bool Exists<T>(Dictionary<dynamic,dynamic> properties) where T:class;
         Task<bool> Exists<T>(Expression<Func<T,bool>> expression = null) where T:class;
         Task<bool> Exists<T>(T source) where T:class;
         Task<bool> SaveAll();
    }
}