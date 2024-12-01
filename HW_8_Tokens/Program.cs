var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseToken("pass");
//app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RoutingMiddleware>();

app.Run();


//https://localhost:7124/
//https://localhost:7124/page1?token=pass
public class RoutingMiddleware
{

	public RoutingMiddleware(RequestDelegate _) { }

	public async Task InvokeAsync(HttpContext context)
	{
		string path = context.Request.Path;
		if (path == "/")
		{
			await PageConfig(context, "index.html");
		}
		else if (path == "/page1")
		{
			await PageConfig(context, "page1.html");
		}
		else if (path == "/page2")
		{
			await PageConfig(context, "page2.html");
		}
		else if (path == "/page3")
		{
			await PageConfig(context, "page3.html");
		}
		else if (path == "/page4")
		{
			await PageConfig(context, "page4.html");
		}
		else
		{
			context.Response.StatusCode = 404;
		}
	}

	async Task PageConfig(HttpContext context, string fileName)
	{
		var path = context.Request.Path;
		var folderPath = $"html/";
		var response = context.Response;
		response.ContentType = "text/html; charset=utf-8";
		await response.SendFileAsync($"{folderPath}/{fileName}");
	}
}



public class AuthenticationMiddleware
{
	readonly RequestDelegate next;
	string value;
	public AuthenticationMiddleware(RequestDelegate next, string value)
	{
		this.next = next;
		this.value = value;
	}
	public async Task InvokeAsync(HttpContext context)
	{
		var token = context.Request.Query["token"];
		if (string.IsNullOrWhiteSpace(token) && context.Request.Path != "/")
		{
			context.Response.StatusCode = 403;
		}
		else if (token != value && context.Request.Path != "/")
		{
			context.Response.StatusCode = 403;
		}
		else
		{
			await next.Invoke(context);
		}
	}
}

public class ErrorHandlingMiddleware
{
	readonly RequestDelegate next;
	public ErrorHandlingMiddleware(RequestDelegate next)
	{
		this.next = next;
	}
	public async Task InvokeAsync(HttpContext context)
	{
		await next.Invoke(context);
		if (context.Response.StatusCode == 403)
		{
			context.Response.ContentType = "text/html; charset=utf-8";
			await context.Response.WriteAsync("Access Denied (Доступ запрещен)");
		}
		else if (context.Response.StatusCode == 404)
		{
			context.Response.ContentType = "text/html; charset=utf-8";
			await context.Response.WriteAsync("Not Found (Страница не найдена)");
		}
	}
}

public static class TokenExtensions
{
	public static IApplicationBuilder UseToken(this IApplicationBuilder builder, string value)
	{
		return builder.UseMiddleware<AuthenticationMiddleware>(value);
	}
}
