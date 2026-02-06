namespace TunnlR.Contract.DTOs.Auth
{
    public class HttpRequestData
    {
        public Guid RequestId { get; set; } 
        public string Method { get; set; } = string.Empty; 
        public string Path { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = string.Empty;
    }
}
