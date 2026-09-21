using Dms.Domain.Common;
using Dms.Domain.Interfaces;
using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dms.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AsNoTracking().Where(predicate).ToListAsync();
        }

        public async Task<PagedResult<T>> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            // 1. Dùng AsNoTracking() để không tốn RAM và CPU cho ChangeTracker
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // 2. CountAsync chạy nhanh trên query gốc không cần Include
            var totalCount = await query.CountAsync();

            // 3. Nạp trước các quan hệ liên kết (Eager Loading) để tránh lỗi N+1 Query
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // 4. Phân trang dữ liệu
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalCount, pageIndex, pageSize);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
                if (key != null)
                {
                    var keyValues = key.Properties.Select(p => p.PropertyInfo?.GetValue(entity)).ToArray();
                    var existing = _context.ChangeTracker.Entries<T>().FirstOrDefault(e =>
                    {
                        var eKeyValues = key.Properties.Select(p => p.PropertyInfo?.GetValue(e.Entity)).ToArray();
                        return keyValues.SequenceEqual(eKeyValues);
                    });

                    if (existing != null)
                    {
                        existing.State = EntityState.Detached;
                    }
                }
            }
            _context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.LastModified = DateTime.UtcNow;
                Update(entity);
            }
            else
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
                    if (key != null)
                    {
                        var keyValues = key.Properties.Select(p => p.PropertyInfo?.GetValue(entity)).ToArray();
                        var existing = _context.ChangeTracker.Entries<T>().FirstOrDefault(e =>
                        {
                            var eKeyValues = key.Properties.Select(p => p.PropertyInfo?.GetValue(e.Entity)).ToArray();
                            return keyValues.SequenceEqual(eKeyValues);
                        });

                        if (existing != null)
                        {
                            existing.State = EntityState.Detached;
                        }
                    }
                }
                _context.Set<T>().Remove(entity);
            }
        }
    }
}
