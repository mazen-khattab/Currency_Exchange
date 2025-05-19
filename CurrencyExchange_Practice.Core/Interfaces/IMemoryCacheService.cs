using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyExchange_Practice.Core.Interfaces
{
    public interface IMemoryCacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration);
        void RemovePrefix(string prefix);
        void Remove(string key);
    }
}
