using Microsoft.AspNetCore.Http;

namespace Free.RateLimit
{
    public class RateLimitHeaders
    {
        public RateLimitHeaders(HttpContext context,string limit,string remaining,string reset) {
            this.Context = context;
            this.Limit = limit;
            this.Remaining = remaining;
            this.Reset = reset;
        }

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext Context { get;private set; }
        /// <summary>
        ///     限制
        /// </summary>
        public string Limit { get;private set; }
        /// <summary>
        ///     剩下的
        /// </summary>
        public string Remaining { get;private set; }
        /// <summary>
        ///    重置
        /// </summary>
        public string Reset { get;private set; }

    }
}
