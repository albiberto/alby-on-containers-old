using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Infrastructure;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Repository
{
    public interface IRepository<T> where T : EntityBase
    {
        IUnitOfWork UnitOfWork { get; }

        Task<T> FindAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync(Predicate<T>? selector = default);

        Task<EntityEntry<T>> AddAsync(T entity);
        Task<EntityEntry<T>> UpdateAsync(T entity);
        Task<EntityEntry<T>> DeleteAsync(T entity);

        Task AddRangeAsync(IEnumerable<T> entity);
        Task UpdateRangeAsync(IEnumerable<T> entity);
        Task DeleteRangeAsync(IEnumerable<T> entity);
    }

    public class RepositoryBase<T> : IRepository<T> where T : EntityBase
    {
        protected readonly LuciferContext _context;

        public RepositoryBase(LuciferContext context)
        {
            UnitOfWork = context;
            _context = context;
        }

        public virtual async Task<T> FindAsync(Guid id) => await _context.Set<T>().FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync(Predicate<T>? selector = default)
        {
            var result = selector == default
                ? _context.Set<T>()
                : _context.Set<T>().Where(e => selector(e));

            return await result.ToListAsync();
        }

        public IUnitOfWork UnitOfWork { get; }

        public virtual async Task<EntityEntry<T>> AddAsync(T entity) => await _context.AddAsync(entity);
        public virtual Task<EntityEntry<T>> UpdateAsync(T entity) => Task.FromResult(_context.Update(entity));
        public virtual Task<EntityEntry<T>> DeleteAsync(T entity) => Task.FromResult(_context.Remove(entity));

        public virtual Task AddRangeAsync(IEnumerable<T> entity) => _context.AddRangeAsync(entity);

        public virtual Task UpdateRangeAsync(IEnumerable<T> entity)
        {
            _context.UpdateRange(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<T> entity)
        {
            _context.RemoveRange(entity);
            return Task.CompletedTask;
        }
    }
}