namespace Free.RateLimit
{
    public class RateLimitRule
    {
        /// <summary>
        ///     HTTP verb and path 
        /// </summary>
        /// <example>
        /// get:/api/values
        /// *:/api/values
        /// *
        /// </example>
        public string Endpoint { get; set; }
        /// <summary>
        ///  标识限流作用于的时间段， 例如： 1s, 5m, 1h,1d 等。如果在这个时间段内访问的次数超过了限制，需要等
        /// </summary>
        public string Period { get; set; }
        /// <summary>
        ///      单位为秒，这个值标识要多少秒后才能重试。
        /// </summary>
        public int PeriodTimespan { get; set; }
        /// <summary>
        ///     时间段内客户端可以发出的最大请求数
        /// </summary>
        public double Limit { get; set; }
    }
}
