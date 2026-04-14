namespace SIFS.Shared.Extensions
{
    public interface IFileUrlBuilder
    {
        string ToAbsoluteUrl(string relativePath);
    }
}
