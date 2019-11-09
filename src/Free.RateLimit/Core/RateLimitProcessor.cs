using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Free.RateLimit
{
    public  class RateLimitProcessor
    {
        private readonly IRateLimitStore _rateLimitStore;
        private static readonly object _processLocker = new object();
        public RateLimitProcessor(IRateLimitStore rateLimitStore) {
            _rateLimitStore = rateLimitStore;
        }

        public async Task<RateLimitCounter> ProcessRequest(ClientRequestIdentity requestIdentity, RateLimitOptions option) {
            RateLimitCounter counter = new RateLimitCounter(DateTime.UtcNow, 1);
            var rule = option.RateLimitRule;

            var counterId = ComputeCounterKey(requestIdentity, option);

            // serial reads and writes
            lock (_processLocker)
            {
                var entry = _rateLimitStore.Get(counterId);
                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + TimeSpan.FromSeconds(rule.PeriodTimespan) >= DateTime.UtcNow)
                    {
                        // increment request count
                        var totalRequests = entry.Value.TotalRequests + 1;

                        // deep copy
                        counter = new RateLimitCounter(entry.Value.Timestamp, totalRequests);
                    }
                }
            }

            if (counter.TotalRequests > rule.Limit)
            {
                var retryAfter = RetryAfterFrom(counter.Timestamp, rule);
                if (retryAfter > 0)
                {
                    var expirationTime = rule.PeriodTimespan;
                  await _rateLimitStore.SetAsync(counterId, counter, TimeSpan.FromSeconds(expirationTime));
                }
                else
                {
                  await  _rateLimitStore.RemoveAsync(counterId);
                }
            }
            else
            {
                var expirationTime = ConvertToTimeSpan(rule.Period);
                await _rateLimitStore.SetAsync(counterId, counter, expirationTime);
            }

            return counter;
        }
        public Task SaveRateLimitCounter(ClientRequestIdentity requestIdentity, RateLimitOptions option, RateLimitCounter counter, TimeSpan expirationTime)
        {
            var counterId = ComputeCounterKey(requestIdentity, option);

            // stores: id (string) - timestamp (datetime) - total_requests (long)
            _rateLimitStore.SetAsync(counterId, counter, expirationTime);
            return Task.CompletedTask;
        }
        public RateLimitHeaders GetRateLimitHeaders(HttpContext context,ClientRequestIdentity requestIdentity,RateLimitOptions option) {
            var rule = option.RateLimitRule;
            RateLimitHeaders headers = null;
            var counterId = ComputeCounterKey(requestIdentity, option);
            var entry = _rateLimitStore.Get(counterId);
            if (entry.HasValue)
            {
                headers = new RateLimitHeaders(context, rule.Period,
                    (rule.Limit - entry.Value.TotalRequests).ToString(),
                    (entry.Value.Timestamp + ConvertToTimeSpan(rule.Period)).ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo)
                    );
            }
            else
            {
                headers = new RateLimitHeaders(context,
                    rule.Period,
                    rule.Limit.ToString(),
                    (DateTime.UtcNow + ConvertToTimeSpan(rule.Period)).ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo));
            }

            return headers;
        }

        public string ComputeCounterKey(ClientRequestIdentity requestIdentity,RateLimitOptions options) {
            var key = $"{options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{options.RateLimitRule.Period}_{requestIdentity.HttpVerb}_{requestIdentity.Path}";
            var idBytes = Encoding.UTF8.GetBytes(key);
            byte[] hashBytes;
            using (var algorithm=SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        public int RetryAfterFrom(DateTime timestamp,RateLimitRule rule) {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow-timestamp).TotalSeconds);
            var retryAfter = Convert.ToInt32(rule.PeriodTimespan);
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter;
        }
        public TimeSpan ConvertToTimeSpan(string timeSpan) {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0,l);
            var type = timeSpan.Substring(l,1);
            switch (type)
            {
                case "d":
                    return TimeSpan.FromDays(double.Parse(value));

                case "h":
                    return TimeSpan.FromHours(double.Parse(value));

                case "m":
                    return TimeSpan.FromMinutes(double.Parse(value));

                case "s":
                    return TimeSpan.FromSeconds(double.Parse(value));

                default:
                    throw new FormatException($"{timeSpan} can't be converted to TimeSpan, unknown type {type}");
            }
        }

    }
}
