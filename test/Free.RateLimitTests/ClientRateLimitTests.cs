using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Free.RateLimitTests
{
    public class ClientRateLimitTests
    {
        private const string apiPath = "/WeatherForecast";
        public static HttpClient Client()
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new System.Uri("http://localhost:35830")
            };
            return client;
        }

        [Theory]
        [InlineData("GET", "cl-key-3")]
        [InlineData("POST", "cl-key-4")]
        public async Task SpecificClientRule(string verb,string clientId)
        {
            // Arrange
            int responseStatusCode = 0;
            // Act    
            for (int i = 0; i < 3; i++)
            {
                var request = new HttpRequestMessage(new HttpMethod(verb), apiPath);

                request.Headers.Add("X-ClientId", clientId);
                var response = await Client().SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }
            // Assert
            Assert.Equal(429, responseStatusCode);
        }

        [Fact]
        public async Task SpecificPathRule()
        {
            // Arrange
            var clientId = "cl-key-3";
            int responseStatusCode = 0;
            var content = string.Empty;
            var keyword = "10s";

            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
                request.Headers.Add("X-ClientId", clientId);

                var response = await Client().SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal(429, responseStatusCode);
            Assert.Contains(keyword, content);
        }
        [Fact]
        public async Task WhitelistClient()
        {
            // Arrange
            var clientId = "cl-key-1";
            int responseStatusCode = 0;
            // Act    
            for (int i = 0; i < 4; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Add("X-ClientId", clientId);

                var response = await Client().SendAsync(request);
                responseStatusCode = (int)response.StatusCode;
            }
            // Assert
            Assert.Equal(200, responseStatusCode);
        }

    }
}
