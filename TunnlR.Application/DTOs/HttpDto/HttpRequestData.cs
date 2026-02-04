namespace TunnlR.Application.DTOs.HttpDto
{
    public class HttpRequestData
    {
        public string Method { get; set; } = string.Empty; 
        public string Path { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = string.Empty;
    }
}
