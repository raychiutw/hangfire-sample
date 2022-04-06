using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace HangfireWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValueController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<ValueController> _logger;
        private readonly IBackgroundJobClient _jobClient;

        public ValueController(
            ILogger<ValueController> logger,
            IBackgroundJobClient jobClient)
        {
            _logger = logger;
            _jobClient = jobClient;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> GetAsync()
        {
            _jobClient.Enqueue<ISystemJob>(x => x.RemoveHttpLogAsync(null, 1));

            return Ok();
        }
    }
}