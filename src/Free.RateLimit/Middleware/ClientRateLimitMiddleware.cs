using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Free.RateLimit
{
    public class ClientRateLimitMiddleware : RateLimitProcessor
    {
        private readonly ILogger<ClientRateLimitMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IRateLimitStore _rateLimitStore;
        private readonly RateLimitOptions _options;
        public ClientRateLimitMiddleware(RequestDelegate next,
            ILogger<ClientRateLimitMiddleware> logger,
            IRateLimitStore rateLimitStore, IOptions<RateLimitOptions> options) : base(rateLimitStore)
        {
            _next = next;
            _rateLimitStore = rateLimitStore;
            _logger = logger;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if rate limiting is enabled
            if (!_options.EnableRateLimiting)
            {
                _logger.LogInformation($"EndpointRateLimiting is not enabled for ");
                await _next.Invoke(context);
                return;
            }

            // compute identity from request
            var identity = SetIdentity(context, _options);

            // check white list
            if (IsWhitelisted(identity, _options))
            {
                _logger.LogInformation($" is white listed from rate limiting");
                await _next.Invoke(context);
                return;
            }

            var rule = _options.RateLimitRule;
            if (rule.Limit > 0)
            {
                // increment counter
                var counter = (await ProcessRequest(identity, _options));

                // check if limit is reached
                if (counter.TotalRequests > rule.Limit)
                {
                    //compute retry after value
                    var retryAfter = RetryAfterFrom(counter.Timestamp, rule);

                    // log blocked request
                    LogBlockedRequest(context, identity, counter, rule);

                    var retrystring = retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    // break execution
                    await ReturnQuotaExceededResponse(context, _options, retrystring);

                    return;
                }
            }

            //set X-Rate-Limit headers for the longest period
            if (!_options.DisableRateLimitHeaders)
            {
                var headers = GetRateLimitHeaders(context, identity, _options);
                context.Response.OnStarting(SetRateLimitHeaders, state: headers);
            }

            await _next.Invoke(context);
        }

        public ClientRequestIdentity SetIdentity(HttpContext httpContext, RateLimitOptions option)
        {
            var clientId = "client";
            if (httpContext.Request.Headers.Keys.Contains(option.ClientIdHeader))
            {
                clientId = httpContext.Request.Headers[option.ClientIdHeader];
            }
            return new ClientRequestIdentity(
                clientId,
                httpContext.Request.Path.ToString().ToLowerInvariant(),
                httpContext.Request.Method.ToLowerInvariant()
                );
        }

        public bool IsWhitelisted(ClientRequestIdentity requestIdentity, RateLimitOptions option)
        {
            if (option.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }
            return false;
        }

        public void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation(
                $"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. TraceIdentifier {httpContext.TraceIdentifier}.");
        }

        public Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitOptions option, string retryAfter)
        {
            var message = this.GetResponseMessage(option);

            if (!option.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }
            httpContext.Response.StatusCode = option.HttpStatusCode;
            return httpContext.Response.WriteAsync(message);
        }

        private string GetResponseMessage(RateLimitOptions option)
        {
            var message = string.IsNullOrEmpty(option.QuotaExceededMessage)
                ? $"API calls quota exceeded! maximum admitted {option.RateLimitRule.Limit} per {option.RateLimitRule.Period}."
                : option.QuotaExceededMessage;
            return message;
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}
