// <copyright file="RequestInfoLoggingMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.Infrastructure.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Logs incoming request details including method, URL, IP, headers, body and current user id.
/// </summary>
public sealed class RequestInfoLoggingMiddleware
{
    private const int MaxLoggedBodyLength = 4000;
    private readonly RequestDelegate next;
    private readonly ILogger<RequestInfoLoggingMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestInfoLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next middleware in the pipeline.</param>
    /// <param name="logger">Logger instance.</param>
    public RequestInfoLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestInfoLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Executes middleware logic.
    /// </summary>
    /// <param name="httpContext">Current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var requestBody = await ReadRequestBodyAsync(request);
        var headers = request.Headers.ToDictionary(
            header => header.Key,
            header => header.Value.ToString());

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        this.logger.LogInformation(
            "HTTP request: {Method} {Url} from {IpAddress}. UserId: {UserId}. Headers: {@Headers}. Body: {Body}",
            request.Method,
            url,
            ipAddress,
            userId ?? "Anonymous",
            headers,
            string.IsNullOrWhiteSpace(requestBody) ? "<empty>" : requestBody);

        await this.next(httpContext);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        if (request.Body.CanSeek)
        {
            request.Body.Position = 0;
        }

        if (request.ContentLength is null or <= 0)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        if (request.Body.CanSeek)
        {
            request.Body.Position = 0;
        }

        return content.Length <= MaxLoggedBodyLength
            ? content
            : content[..MaxLoggedBodyLength] + "... [truncated]";
    }
}
