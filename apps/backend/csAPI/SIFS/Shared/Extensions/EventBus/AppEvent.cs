namespace SIFS.Shared.Extensions.EventBus
{
    public class AppEvent
    {
        public string EventType { get; set; } = string.Empty;

        public Guid? ActorId { get; set; }

        public string? TargetType { get; set; }

        public string? TargetId { get; set; }

        public Dictionary<string, object?>? Payload { get; set; }

        public Dictionary<string, object?>? RequestContext { get; set; }

        public bool Success { get; set; } = true;

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
