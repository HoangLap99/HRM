namespace HRM.Api.Common;

/// <summary>
/// Lớp tương thích thay thế System.Web.Hosting.HostingEnvironment.MapPath trên .NET 8.
/// </summary>
public static class HostingEnvironmentCompat
{
    private static string _contentRootPath = AppContext.BaseDirectory;
    private static string _legacyFallbackRoot = @"e:\HRM\backend\HRMNew";
    public static string PathUploads { get; private set; } = @"C:\uploader\App_Data\uploads\";
    public static string PathUploadsGoc { get; private set; } = @"C:\uploader\App_Data\uploads\";

    public static void Configure(string contentRootPath, string? legacyFallbackRoot = null, string? pathUploads = null, string? pathUploadsGoc = null)
    {
        if (!string.IsNullOrWhiteSpace(contentRootPath))
        {
            _contentRootPath = contentRootPath;
        }

        if (!string.IsNullOrWhiteSpace(legacyFallbackRoot))
        {
            _legacyFallbackRoot = legacyFallbackRoot;
        }
        if (!string.IsNullOrWhiteSpace(pathUploads))
        {
            PathUploads = pathUploads;
        }
        if (!string.IsNullOrWhiteSpace(pathUploadsGoc))
        {
            PathUploadsGoc = pathUploadsGoc;
        }
    }

    public static string MapPath(string? virtualPath)
    {
        if (string.IsNullOrWhiteSpace(virtualPath))
        {
            return _contentRootPath;
        }

        var trimmed = virtualPath.Trim();
        var relative = trimmed
            .TrimStart('~')
            .TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var primaryPath = Path.Combine(_contentRootPath, relative);
        if (File.Exists(primaryPath) || Directory.Exists(primaryPath))
        {
            return primaryPath;
        }

        if (!string.IsNullOrWhiteSpace(_legacyFallbackRoot))
        {
            var fallbackPath = Path.Combine(_legacyFallbackRoot, relative);
            if (File.Exists(fallbackPath) || Directory.Exists(fallbackPath))
            {
                return fallbackPath;
            }
        }

        return primaryPath;
    }
}
