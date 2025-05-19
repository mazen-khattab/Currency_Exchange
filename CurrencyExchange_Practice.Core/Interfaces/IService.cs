using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Specification;

namespace CurrencyExchange_Practice.Core.Interfaces
{
    public interface IService<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true, params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
