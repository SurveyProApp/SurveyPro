// <copyright file="RequestExecutionTimeLoggingMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.Infrastructure.Middleware;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Logs total request execution time.
/// </summary>
public sealed class RequestExecutionTimeLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<RequestExecutionTimeLoggingMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestExecutionTimeLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next middleware in the pipeline.</param>
    /// <param name="logger">Logger instance.</param>
    public RequestExecutionTimeLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestExecutionTimeLoggingMiddleware> logger)
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
        var stopwatch = Stopwatch.StartNew();
        await this.next(httpContext);
        stopwatch.Stop();

        this.logger.LogInformation(
            "HTTP request completed: {Method} {Path} with status code {StatusCode} in {ElapsedMilliseconds} ms",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}
