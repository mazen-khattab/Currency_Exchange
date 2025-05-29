using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Interfaces;
using CurrencyExchange_Practice.Core.Specification;

namespace CurrencyExchange_Practice.Application.Services
{
    public class Service<T> : IService<T> where T : BaseEntity
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepo<T> _repo;

        public Service(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repo = unitOfWork.GetRepo<T>();
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true, params Expression<Func<T, object>>[] includes) => await _repo.GetAll(filter, tracked, includes);

        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes) => await _repo.GetById(id, includes);

        public async Task AddAsync(T entity)
        {
            await _repo.Create(entity);
            await _unitOfWork.SaveChanges();
        }

        public async Task UpdateAsync(T entity)
        {
            await _repo.Update(entity);
            await _unitOfWork.SaveChanges();
        }

        public async Task DeleteAsync(T entity)
        {
            await _repo.Remove(entity);
            await _unitOfWork.SaveChanges();
        }
    }
}
