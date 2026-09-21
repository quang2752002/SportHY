using Dms.Domain.Common;
using System.Linq.Expressions;

namespace Dms.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<PagedResult<T>> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
