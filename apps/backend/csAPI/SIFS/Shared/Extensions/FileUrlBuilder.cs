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
            return BuildUrl(_options.BaseUrl, relativePath, "BaseUrl");
        }

        public string ToPythonUrl(string relativePath)
        {
            return BuildUrl(_options.PyBaseUrl, relativePath, "PyBaseUrl");
        }

        private string BuildUrl(string baseUrl, string relativePath, string optionName)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("relativePath 不能为空", nameof(relativePath));

            if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            baseUrl = baseUrl?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException($"AppUrlOptions:{optionName} 未配置");

            var path = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;

            return baseUrl + path;
        }

    }
}
