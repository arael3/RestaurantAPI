using System.Diagnostics;

namespace RestaurantAPI.Middleware
{
    public class RequestTimeMiddleware : IMiddleware
    {
        private Stopwatch _stopWatch;
        private readonly ILogger<RequestTimeMiddleware> _logger;

        public RequestTimeMiddleware(ILogger<RequestTimeMiddleware> logger)
        {
            _stopWatch = new Stopwatch();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _stopWatch.Start();
            await next.Invoke(context);
            _stopWatch.Stop();
            float time = _stopWatch.ElapsedMilliseconds;

            if (time > 2)
            {
                _logger.LogInformation($"Request of [{context.Request.Method}] method at {context.Request.Path} time is {time} ms");
            }
        }
    }
}
