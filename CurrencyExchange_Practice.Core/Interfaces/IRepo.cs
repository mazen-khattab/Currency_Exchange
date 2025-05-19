using System.Linq.Expressions;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Specification;

namespace CurrencyExchange_Practice.Core.Interfaces
{
    public interface IRepo<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, bool tracked = true, params Expression<Func<T, object>>[] includes);
        Task<T?> GetById(int id, params Expression<Func<T, object>>[] includes);
        Task Create(T entity);
        Task Update(T entity);
        Task Remove(T entity);
    }
}
