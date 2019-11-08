using Newtonsoft.Json;
using System;

namespace Free.RateLimit
{
    /// <summary>
    ///     初始化时间和请求数量
    /// </summary>
    public struct RateLimitCounter
    {
        [JsonConstructor]
        public RateLimitCounter(DateTime timestamp,double totalRequests) {
            this.Timestamp = timestamp;
            this.TotalRequests = totalRequests;
        }

        /// <summary>
        ///     初始访问时间
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        ///     请求数
        /// </summary>
        public double TotalRequests { get; set; }
    }
}
