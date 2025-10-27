using System.Linq.Expressions;
using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        //Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
