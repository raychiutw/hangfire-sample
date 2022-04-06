namespace HangfireWeb
{
    /// <summary>
    /// interface IHangfireJobTrigger.
    /// </summary>
    public interface IHangfireJobTrigger
    {
        /// <summary>
        /// Called when [start].
        /// </summary>
        Task OnStart();
    }
}