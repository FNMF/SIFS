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
            return ToAbsoluteUrl(relativePath);
        }

        private string BuildUrl(string baseUrl, string relativePath, string optionName)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("relativePath 不能为空", nameof(relativePath));

            baseUrl = baseUrl?.TrimEnd('/');

            if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (TryRewriteLocalAbsoluteUrl(relativePath, baseUrl, out var rewrittenUrl))
                    return rewrittenUrl;

                return relativePath;
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException($"AppUrlOptions:{optionName} 未配置");

            var path = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;

            return baseUrl + path;
        }

        private static bool TryRewriteLocalAbsoluteUrl(string absoluteUrl, string? baseUrl, out string rewrittenUrl)
        {
            rewrittenUrl = absoluteUrl;

            if (string.IsNullOrWhiteSpace(baseUrl) ||
                !Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var isLocalHost =
                uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);

            if (!isLocalHost)
                return false;

            rewrittenUrl = baseUrl.TrimEnd('/') + uri.PathAndQuery;
            return true;
        }

    }
}
