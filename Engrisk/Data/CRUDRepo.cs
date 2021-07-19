using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Helper;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Engrisk.Data
{
    public class CRUDRepo : ICRUDRepo
    {
        private readonly ApplicationDbContext _db;
        public CRUDRepo(ApplicationDbContext db)
        {
            _db = db;
        }
        public void Create<T>(T subject) where T : class
        {
            _db.Add(subject);
        }

        public void Delete<T>(dynamic id) where T : class
        {
            var dbSet = _db.Set<T>();
            var subject = dbSet.FindAsync(id);
            dbSet.Remove(subject);
        }
        public void Delete<T>(T subject) where T : class
        {
            _db.Remove(subject);
        }

        public void Delete<T>(IEnumerable<T> subjects) where T : class
        {
            var _dbSet = _db.Set<T>();
            foreach (var subject in subjects)
            {
                _dbSet.Remove(subject);
            }
        }

        public void Delete<T>() where T : class
        {
            try
            {
                var dbSet = _db.Set<T>();
                foreach (var item in dbSet.ToList())
                {
                    dbSet.Remove(item);
                }
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }

        public bool Exists<T>(Dictionary<dynamic, dynamic> properties) where T : class
        {
            var _dbSet = _db.Set<T>();
            if (_dbSet.Count() == 0)
            {
                return false;
            }
            foreach (var item in _dbSet.AsNoTracking().ToList())
            {
                if (item != null)
                {
                    if (item.CompareProperties(properties))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> Exists<T>(T source) where T : class
        {
            try
            {
                var dbSet = _db.Set<T>();
                var temp = await dbSet.ToListAsync();
                if (temp.Contains(source))
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }

        public async Task<bool> Exists<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            var _dbSet = _db.Set<T>();
            return await _dbSet.AnyAsync(expression);
        }

        // public async Task<PagingList<T>> GetAll<T>(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, SubjectParams subjectParams = null, string includeProperties = "") where T : class
        // {
        //     var dbSet = _db.Set<T>();
        //     var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
        //     if(expression != null)
        //     {
        //         queryableDbSet = queryableDbSet.Where(expression);
        //     }
        //     if(includeProperties != null)
        //     {
        //         foreach(var property in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
        //         {
        //            queryableDbSet =  queryableDbSet.Include(property);
        //         }
        //     }
        //     if(orderBy != null)
        //     {
        //         queryableDbSet =  orderBy(queryableDbSet);
        //     }

        //     return await PagingList<T>.OnCreateAsync(queryableDbSet, subjectParams.CurrentPage, subjectParams.PageSize);
        // }

        public async Task<PagingList<T>> GetAll<T>(SubjectParams subjectParams = null, Expression<Func<T, bool>> expression = null, string includeProperties = "", Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null) where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableDbSet = queryableDbSet.Include(property.Trim());
                }
            }
            if (orderBy != null)
            {
                queryableDbSet = orderBy(queryableDbSet);
            }
            return await PagingList<T>.OnCreateAsync(queryableDbSet, subjectParams.CurrentPage, subjectParams.PageSize);
        }
        public async Task<List<T>> GetAll<T>(Expression<Func<T, bool>> expression = null, string includeProperties = "", Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null) where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableDbSet = queryableDbSet.Include(property.Trim());
                }
            }

            if (orderBy != null)
            {
                queryableDbSet = orderBy(queryableDbSet);
            }
            return await queryableDbSet.ToListAsync();
        }
        public async Task<T> GetOneWithCondition<T>(Expression<Func<T, bool>> expression, string includeProperties) where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableDbSet = queryableDbSet.Include(property.Trim());
                }
            }
            return await queryableDbSet.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<T> GetOneWithConditionTracking<T>(Expression<Func<T, bool>> expression = null, string includeProperties = "") where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableDbSet = queryableDbSet.Include(property.Trim());
                }
            }
            return await queryableDbSet.FirstOrDefaultAsync();
        }

        public async Task<T> GetOneWithKey<T>(dynamic key) where T : class
        {
            var _dbSet = _db.Set<T>();
            return await _dbSet.FindAsync(key);
        }

        public async Task<IQueryable<T>> GetOneWithManyToMany<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(await dbSet.CountAsync());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            return queryableDbSet;
        }

        public IEnumerable<T> GetWithProperties<T>(Dictionary<dynamic, dynamic> properties, Expression<Func<T, bool>> expression, string includeProperties = "") where T : class
        {
            var dbSet = _db.Set<T>();
            var queryableDbSet = dbSet.Take(dbSet.Count());
            if (expression != null)
            {
                queryableDbSet = queryableDbSet.Where(expression);
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableDbSet = queryableDbSet.Include(property.Trim());
                }
            }
            foreach (var subject in queryableDbSet)
            {
                if (subject.CompareProperties(properties))
                {
                    yield return subject;
                }
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public void Update<T>(T subject) where T : class
        {
            if (_db.Attach(subject).State == EntityState.Detached)
            {
                _db.Attach(subject);
            }
            _db.Entry(subject).State = EntityState.Modified;
        }
    }
}