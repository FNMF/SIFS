namespace SIFS.Infrastructure.Identity
{
    public interface ICurrentService
    {
        bool IsAuthenticated { get; }
        Guid? CurrentUuid { get; }
        Guid RequiredUuid { get; }
        string? CurrentAccount { get; }
    }
}
