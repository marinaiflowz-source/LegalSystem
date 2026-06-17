namespace LegalSystem.DTOs
{
    public class FileStorageConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string MainBucket { get; set; } = string.Empty;
    }
    public class ConnectionStrings
    {
        public string CRM { get; set; } = string.Empty;
        public string PACTRPT { get; set; } = string.Empty;
    }
}
