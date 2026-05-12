using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ComputerConfigContext dbContext)
    {
        
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);

            
            if (context.User.Identity?.IsAuthenticated == true &&
                !context.Request.Path.StartsWithSegments("/api/auth/login") &&
                !context.Request.Path.StartsWithSegments("/api/auth/register"))
            {
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var userRole = context.User.FindFirstValue(ClaimTypes.Role);
                        var userName = context.User.FindFirstValue(ClaimTypes.Name);

                        var logEntry = new Domain.Entities.AuditLog
                        {
                            UserId = userId,
                            UserRole = userRole,
                            UserName = userName,
                            HttpMethod = context.Request.Method,
                            Path = context.Request.Path,
                            StatusCode = context.Response.StatusCode,
                            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                            Timestamp = DateTime.UtcNow
                        };

                        dbContext.AuditLogs.Add(logEntry);
                        await dbContext.SaveChangesAsync();
                    }
                    catch
                    {
                        
                    }
                });
            }
        }
        finally
        {
            
            context.Response.Body = originalBodyStream;
        }
    }
}