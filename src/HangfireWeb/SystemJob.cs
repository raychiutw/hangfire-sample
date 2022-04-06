using Hangfire.Server;

namespace HangfireWeb
{
    /// <summary>
    /// System Job
    /// </summary>
    /// <seealso cref="ISystemJob"/>
    public class SystemJob : ISystemJob
    {
        private readonly ILogger<SystemJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SystemJob(ILogger<SystemJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 移除超過 N 日的 AutoHistory
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task RemoveHttpLogAsync(PerformContext? context, int day)
        {
            _logger.LogInformation($"Job Id: {context.BackgroundJob.Id}");
            var dateTime = DateTime.Now;
            _logger.LogInformation($"開始: {dateTime}");

            // 實作
            var removeCount = day;

            _logger.LogInformation($"刪除筆數: {removeCount}");
            _logger.LogInformation($"結束: {DateTime.Now}");
        }
    }
}