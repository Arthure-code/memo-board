using System.Linq.Expressions;
using MemoBoard.Core.Entities;

namespace MemoBoard.Core.Interfaces
{
    // The one door to the database, the same for every entity.
    public interface IAsyncRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}
