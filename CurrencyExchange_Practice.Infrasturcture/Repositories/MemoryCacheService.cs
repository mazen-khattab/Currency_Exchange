using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange_Practice.Infrasturcture.Repositories
{
    public class MemoryCacheService : IMemoryCacheService
    {
        readonly IMemoryCache _memoryCache;
        readonly ConcurrentDictionary<string, byte> _keys = new();

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }


        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration)
        {
            if (!_memoryCache.TryGetValue(key, out T value))
            {
                value = await factory();
                _memoryCache.Set(key, value, duration);
                _keys?.TryAdd(key, 0);
            }

            Console.WriteLine("--------- cache keys ---------");

            foreach (var item in _keys.Keys)
            {
                Console.WriteLine(item);
            }

            return value;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        public void RemovePrefix(string prefix)
        {
            var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix)).ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
            }
        }
    }
}
