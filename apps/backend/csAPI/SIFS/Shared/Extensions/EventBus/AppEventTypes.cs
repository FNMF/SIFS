namespace SIFS.Shared.Extensions.EventBus
{
    public static class AppEventTypes
    {
        public const string UserLogin = "USER_LOGIN";
        public const string TaskCreated = "TASK_CREATED";
        public const string TaskViewed = "TASK_VIEWED";
        public const string TaskDeleted = "TASK_DELETED";
        public const string TaskRetried = "TASK_RETRIED";
        public const string TaskStatusChanged = "TASK_STATUS_CHANGED";
        public const string AlgoCreated = "ALGO_CREATED";
        public const string AlgoUpdated = "ALGO_UPDATED";
        public const string AlgoEnabled = "ALGO_ENABLED";
        public const string AlgoDisabled = "ALGO_DISABLED";
        public const string AlgoHealthChanged = "ALGO_HEALTH_CHANGED";
        public const string ResultDownloaded = "RESULT_DOWNLOADED";

        public static readonly IReadOnlyList<string> All = new[]
        {
            UserLogin,
            TaskCreated,
            TaskViewed,
            TaskDeleted,
            TaskRetried,
            AlgoCreated,
            AlgoUpdated,
            AlgoEnabled,
            AlgoDisabled,
            ResultDownloaded
        };
    }
}
