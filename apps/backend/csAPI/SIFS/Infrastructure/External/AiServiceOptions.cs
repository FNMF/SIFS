namespace SIFS.Infrastructure.External
{
    public class AiServiceOptions
    {
        public Dictionary<AiServiceType, string> Endpoints { get; set; } = new();
    }
}
