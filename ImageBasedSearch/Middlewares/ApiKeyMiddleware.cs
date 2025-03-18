using System.Net;

namespace ImageBasedSearch.Middlewares
{
	public class ApiKeyMiddleware
	{
		private readonly RequestDelegate _next;

		public ApiKeyMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, IConfiguration config)
		{
			var headers = context.Request.Headers;
			if (!headers.TryGetValue("x-api-key", out var key))
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				await context.Response.WriteAsync("Header x-api-key is required!");
				return;
			}

			var apiKey = config["ApiKey"];
			if (key != apiKey)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				await context.Response.WriteAsync("Invalid api key!");
				return;
			}

			await _next(context);
		}
	}
}
