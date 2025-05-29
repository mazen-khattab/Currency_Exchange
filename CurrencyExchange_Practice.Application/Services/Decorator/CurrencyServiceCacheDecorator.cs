using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Interfaces;

namespace CurrencyExchange_Practice.Application.Services.Decorator
{
    public class CurrencyServiceCacheDecorator : ICurrencyService
    {
        readonly IMemoryCacheService _cache;
        readonly ICurrencyService _currency;

        public CurrencyServiceCacheDecorator(IMemoryCacheService memoryCacheService, ICurrencyService currencyService)
        {
            _cache = memoryCacheService;
            _currency = currencyService;
        }


        public async Task<IEnumerable<Currency>> GetAsync(Expression<Func<Currency, bool>>? filter = null, bool tracked = true, params Expression<Func<Currency, object>>[] includes)
        {
            if (includes?.Any() == true)
            {
                return await _currency.GetAsync();
            }

            string key = $"Currency.GetAll";

            return await _cache.GetOrCreateAsync(key, () => _currency.GetAsync(filter, tracked, includes), TimeSpan.FromMinutes(5));
        }

        public async Task<Currency> GetByIdAsync(int id, params Expression<Func<Currency, object>>[] includes)
        {
            if (includes?.Any() == true)
            {
                return await _currency.GetByIdAsync(id, includes);
            }

            string key = $"Currency.GetById.{id}";

            return await _cache.GetOrCreateAsync(key, () => _currency.GetByIdAsync(id), TimeSpan.FromMinutes(5));
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesPaginated(int pageSize, int pageNumber)
        {
            string key = $"Currency.GetPaginated.{pageNumber}";

            return await _cache.GetOrCreateAsync(key, () => _currency.GetAllCurrenciesPaginated(pageSize, pageNumber), TimeSpan.FromMinutes(5));
        }

        public async Task<IEnumerable<Currency>> GetCurrencyByCode(string code)
        {
            string key = $"Currency.GetCurrencyByCode.{code}";

            return await _cache.GetOrCreateAsync(key, () => _currency.GetCurrencyByCode(code), TimeSpan.FromMinutes(5));
        }

        public async Task<Currency> GetCurrencyByCountry(int countryId)
        {
            string key = $"Currency.GetCurrencyByCountry.{countryId}";

            return await _cache.GetOrCreateAsync(key, () => _currency.GetCurrencyByCountry(countryId), TimeSpan.FromMinutes(5));
        }

        public async Task AddAsync(Currency entity)
        { 
            await _currency.AddAsync(entity);

            _cache.Remove("Currency.GetAll");
            _cache.RemovePrefix("Currency.GetPaginated");
        }

        public async Task UpdateAsync(Currency entity)
        {
            await _currency.UpdateAsync(entity);

            RemoveAll(entity);
        }

        public async Task DeleteAsync(Currency entity)
        {
            await _currency.DeleteAsync(entity);

            RemoveAll(entity);
        }

        public void RemoveAll(Currency entity)
        {
            _cache.Remove("Currency.GetAll");
            _cache.Remove($"Currency.GetById.{entity.Id}");
            _cache.Remove($"Currency.GetCurrencyByCode.{entity.CurrencyCode}");
            _cache.RemovePrefix($"Currency.GetCurrencyByCountry");
            _cache.RemovePrefix("Currency.GetPaginated");
        }
    }
}
