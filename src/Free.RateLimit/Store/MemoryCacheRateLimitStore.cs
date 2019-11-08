using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Free.RateLimit
{
    public class MemoryCacheRateLimitStore : IRateLimitStore
    {
        private readonly IMemoryCache _cache;
        public MemoryCacheRateLimitStore(IMemoryCache cache) {
            this._cache = cache;
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(_cache.TryGetValue(id,out _));
        }

        public Task<RateLimitCounter> GetAsync(string id)
        {
            if (_cache.TryGetValue(id,out RateLimitCounter stored))
            {
                return Task.FromResult(stored);
            }
            return Task.FromResult(default(RateLimitCounter));
        }

        public RateLimitCounter? Get(string id)
        {
            if (_cache.TryGetValue(id, out RateLimitCounter stored))
            {
                return stored;
            }
            return null;
        }


        public Task RemoveAsync(string id)
        {
            _cache.Remove(id);
            return Task.CompletedTask;
        }

        public Task SetAsync(string id, RateLimitCounter counter, TimeSpan? expirationTime)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            _cache.Set(id, counter, options);

            return Task.CompletedTask;
        }
    }
}
