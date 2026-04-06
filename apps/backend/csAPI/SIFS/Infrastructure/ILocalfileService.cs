namespace SIFS.Infrastructure
{
    public interface ILocalfileService
    {
        Task<string> LocalSaveAsync(IFormFile file);
    }
}
