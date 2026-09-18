using System.Linq.Expressions;
using MemoBoard.Core.Entities;
using MemoBoard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MemoBoard.Infrastructure.Data
{
    public class AsyncRepository<TEntity> : IAsyncRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly MemoContext _context;

        public AsyncRepository(MemoContext context)
        {
            _context = context;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
