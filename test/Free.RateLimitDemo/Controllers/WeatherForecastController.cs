using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Free.RateLimitDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public OkObjectResult Get()
        {
            return Ok("ok");
        }


        [HttpPost]
        public OkObjectResult Post()
        {
            return Ok("ok");
        }
    }
}
