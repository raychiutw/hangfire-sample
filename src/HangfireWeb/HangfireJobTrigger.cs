using Hangfire;
using Hangfire.Storage;

namespace HangfireWeb
{
    /// <summary>
    /// Hangfire JobTrigger.
    /// </summary>
    /// <seealso cref="IHangfireJobTrigger"/>
    public class HangfireJobTrigger : IHangfireJobTrigger
    {
        /// <summary>
        /// Called when [start].
        /// </summary>
        public async Task OnStart()
        {
            // 清除已有的排程工作
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            RecurringJob.AddOrUpdate<ISystemJob>
            (
                s => s.RemoveHttpLogAsync(null, 10),
                "0 3 * * *",
                TimeZoneInfo.Local
            );
        }
    }
}