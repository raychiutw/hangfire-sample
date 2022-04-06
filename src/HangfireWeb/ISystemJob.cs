using Hangfire;
using Hangfire.Server;

namespace HangfireWeb
{
    /// <summary>
    /// System Job 介面
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public interface ISystemJob
    {
        /// <summary>
        /// 移除超過 N 日的 HttpLog
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("移除超過 {1} 日的 HttpLog")]
        Task RemoveHttpLogAsync(PerformContext? context, int day);
    }
}