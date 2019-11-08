using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Free.RateLimit
{
    public class DistributedCacheRateLimitStore : IRateLimitStore
    {
        private readonly IDistributedCache _cache;
        public DistributedCacheRateLimitStore(IDistributedCache cache) {
            this._cache = cache;
        }
        public async Task<bool> ExistsAsync(string id)
        {
            var stored = await _cache.GetStringAsync(id);
            return !string.IsNullOrEmpty(stored);
        }

        public async Task<RateLimitCounter> GetAsync(string id)
        {
            var stored = await _cache.GetStringAsync(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<RateLimitCounter>(stored);
            }
            return default;
        }
        public RateLimitCounter? Get(string id)
        {
            var stored =  _cache.GetString(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<RateLimitCounter>(stored);
            }
            return null;
        }
        public Task RemoveAsync(string id)
        {
            return _cache.RemoveAsync(id);
        }

        public Task SetAsync(string id, RateLimitCounter counter, TimeSpan? expirationTime)
        {
            var options = new DistributedCacheEntryOptions();
            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }
            return _cache.SetStringAsync(id,JsonConvert.SerializeObject(counter),options);
        }
    }
}
