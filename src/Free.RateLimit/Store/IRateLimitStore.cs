using System;
using System.Threading.Tasks;

namespace Free.RateLimit
{
    public interface IRateLimitStore
    {
        Task<bool> ExistsAsync(string id);

        Task<RateLimitCounter> GetAsync(string id);
        RateLimitCounter? Get(string id);
        Task RemoveAsync(string id);

        Task SetAsync(string id,RateLimitCounter counter,TimeSpan? expirationTime);
    }
}
