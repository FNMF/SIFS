namespace SIFS.Shared.Helpers.JWT
{
    public interface IJwtHelper
    {
        string UserGenerateToken(Guid uuid, string? account);
        int GetExpiresMinutes();
    }
}
