using System.Collections.Generic;

namespace Free.RateLimit
{
    public class RateLimitOptions
    {
        public RateLimitRule RateLimitRule { get; set; }

        public List<string> ClientWhitelist { get; set; }

        /// <summary>
        ///     获取或设置保存客户端标识符的HTTP标头，默认为X-ClientId
        /// </summary>
        public string ClientIdHeader { get; set; }

        /// <summary>
        ///     获取或设置当速率限制发生时返回的HTTP状态码，默认值设置为429(请求太多)
        /// </summary>
        public int HttpStatusCode { get; set; }
        /// <summary>
        ///     获取或设置一个值，该值将用作quotaresponse消息的格式化程序。
        ///     如果没有指定，默认值为:
        ///     API调用配额超额!每{1}允许的最大{0}
        /// </summary>
        public string QuotaExceededMessage { get; set; }

        /// <summary>
        /// Gets or sets the counter prefix, used to compose the rate limit counter cache key
        /// </summary>
        public string RateLimitCounterPrefix { get;  set; }
        /// <summary>
        /// Enables endpoint rate limiting based URL path and HTTP verb
        /// </summary>
        public bool EnableRateLimiting { get;  set; }
        /// <summary>
        /// Disables X-Rate-Limit and Rety-After headers
        /// </summary>
        public bool DisableRateLimitHeaders { get;  set; }
    }
}
