using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.BLL.Helpers
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public T? Get<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }
        public void Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            _cache.Set(key, value, expiry ??  DefaultExpiry);
        }
        public void Remove(string key)
        {
            _cache.Remove(key);
        }
        public void RemoveByPrefix(string prefix)
        {
            var keys = new[]
            {
                $"{prefix}_all",
                $"{prefix}_active",
                $"{prefix}_list"
            };
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }
    }
}
