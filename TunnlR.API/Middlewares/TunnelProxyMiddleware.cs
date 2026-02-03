namespace TunnlR.API.Middlewares
{
    public class TunnelProxyMiddleware
    {
        private readonly RequestDelegate _next;

        public TunnelProxyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Your logic will go here

            // For now, just pass to next middleware
            await _next(context);
        }
    }
}
