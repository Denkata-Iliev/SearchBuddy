using ImageBasedSearch.Controllers;
using ImageBasedSearch.Database;
using Microsoft.EntityFrameworkCore;
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

		public async Task InvokeAsync(HttpContext context, SearchBuddyContext dbContext)
		{
			var requestPath = context.Request.Path.ToString();
			if (!requestPath.Contains("/api"))
			{
				await _next(context);
				return;
			}

			var headers = context.Request.Headers;
			if (!headers.TryGetValue("x-api-key", out var key))
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				await context.Response.WriteAsync("Header x-api-key is required!");
				return;
			}

			var apiKeyHeader = key.ToString();
			var apiKeyUser = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKeyHeader);
			if (apiKeyUser is null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				await context.Response.WriteAsync("Invalid api key!");
				return;
			}

			context.Items.Add("current-user", apiKeyUser);

			await _next(context);
		}
	}
}
