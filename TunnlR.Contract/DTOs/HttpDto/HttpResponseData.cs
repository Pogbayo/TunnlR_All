namespace TunnlR.Contract.DTOs.Auth
{
    public class HttpResponseData
    {
        public Guid RequestId { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}
