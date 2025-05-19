using System.Collections.Generic;
using CurrencyExchange_Practice.Core.Entities;

namespace CurrencyExchange_Practice.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepo<T> GetRepo<T>() where T : BaseEntity;
        ICurrencyRepo CurrencyRepo { get; }
        ICountryRepo CountryRepo { get; }
        IRateRepo RateRepo { get; }
        Task<int> SaveChanges();
    }
}
