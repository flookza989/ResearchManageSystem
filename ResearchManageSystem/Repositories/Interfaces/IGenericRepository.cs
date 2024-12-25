using System.Linq.Expressions;

namespace ResearchManageSystem.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetPagedResponseAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, bool trackChanges = false);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        IQueryable<T> GetQueryable();
    }
}