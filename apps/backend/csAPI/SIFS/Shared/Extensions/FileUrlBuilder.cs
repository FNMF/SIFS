using Microsoft.Extensions.Options;

namespace SIFS.Shared.Extensions
{
    public class FileUrlBuilder : IFileUrlBuilder
    {
        private readonly AppUrlOptions _options;

        public FileUrlBuilder(IOptions<AppUrlOptions> options)
        {
            _options = options.Value;
        }

        public string ToAbsoluteUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("relativePath 不能为空", nameof(relativePath));

            if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            var baseUrl = _options.BaseUrl?.TrimEnd('/');
            var path = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("AppUrlOptions:BaseUrl 未配置");

            return baseUrl + path;
        }
    }
}
