namespace LegalSystem.Helpers
{
    public static class FileHelper
    {
        public static readonly Dictionary<string, string> ExtensionToMime =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
            // Documents
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".txt", "text/plain" },
            { ".rtf", "application/rtf" },
            { ".json", "application/json" },
            { ".xml", "application/xml" },

            // Spreadsheets
            { ".csv", "text/csv" },
            { ".tsv", "text/tab-separated-values" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },

            // Presentations
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },

            // Images
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".svg", "image/svg+xml" },
            { ".webp", "image/webp" },

            // Audio
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".ogg", "audio/ogg" },

            // Video
            { ".mp4", "video/mp4" },
            { ".webm", "video/webm" },
            { ".avi", "video/x-msvideo" },
            { ".mov", "video/quicktime" },

            // Archives
            { ".zip", "application/zip" },
            { ".rar", "application/vnd.rar" },
            { ".7z", "application/x-7z-compressed" },
            { ".tar", "application/x-tar" },
            { ".gz", "application/gzip" },

            // Web
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },

            // Other
            { ".exe", "application/vnd.microsoft.portable-executable" },
            { ".bin", "application/octet-stream" },
            { ".apk", "application/vnd.android.package-archive" }
            };
        public static string GetMimeType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream";

            // Ensure the extension starts with a dot
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (ExtensionToMime.TryGetValue(extension, out string mimeType))
                return mimeType;

            return "application/octet-stream"; // Default fallback
        }
        public static string GetBaseUrl(string host, int port, string bucketName)
        {
            var baseUrl = host;
            if (port > 0)
                baseUrl = baseUrl + ":" + port;

            baseUrl = baseUrl + "/" + bucketName + "/";

            return baseUrl;
        }
    }
}
