namespace SIFS.Application.ModelHealthChecks
{
    public static class ModelHealthStatus
    {
        public const string Online = "online";
        public const string Offline = "offline";
        public const string Timeout = "timeout";
    }

    public class ModelHealthStatusQuery
    {
        public string? Status { get; set; }
        public bool? Enabled { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ModelHealthStatusDto
    {
        public int AlgoModelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string ApiUrl { get; set; } = string.Empty;
        public string HealthStatus { get; set; } = string.Empty;
        public int? ResponseTimeMs { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? FailureReason { get; set; }
        public string? Description { get; set; }
    }

    public class ModelHealthSummaryDto
    {
        public int Online { get; set; }
        public int Offline { get; set; }
        public int Timeout { get; set; }
    }
}
