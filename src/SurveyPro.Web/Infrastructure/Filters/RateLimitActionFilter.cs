// <copyright file="RateLimitActionFilter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Limits the number of requests from a single IP within a one-minute window.
/// </summary>
public sealed class RateLimitActionFilter : IAsyncActionFilter
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private static readonly object SyncRoot = new object();
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<RateLimitActionFilter> logger;
    private readonly int maxRequestsPerMinute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitActionFilter"/> class.
    /// </summary>
    /// <param name="memoryCache">Memory cache.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="maxRequestsPerMinute">Maximum requests allowed per minute.</param>
    public RateLimitActionFilter(
        IMemoryCache memoryCache,
        ILogger<RateLimitActionFilter> logger,
        int maxRequestsPerMinute)
    {
        this.memoryCache = memoryCache;
        this.logger = logger;
        this.maxRequestsPerMinute = Math.Max(1, maxRequestsPerMinute);
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var actionName = context.ActionDescriptor.DisplayName
            ?? context.ActionDescriptor.RouteValues["action"]
            ?? "action";
        var cacheKey = $"ratelimit:{actionName}:{ipAddress}";
        var now = DateTimeOffset.UtcNow;
        var windowStart = now - Window;

        int requestCount;

        lock (SyncRoot)
        {
            var timestamps = this.memoryCache.GetOrCreate(cacheKey, _ => new List<DateTimeOffset>());

            timestamps ??= new List<DateTimeOffset>();

            timestamps.RemoveAll(timestamp => timestamp < windowStart);
            timestamps.Add(now);

            this.memoryCache.Set(cacheKey, timestamps, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Window,
            });

            requestCount = timestamps.Count;
        }

        if (requestCount > this.maxRequestsPerMinute)
        {
            var routeValues = new { retryAfterSeconds = 60 };

            this.logger.LogWarning(
                "Rate limit exceeded for IP {IpAddress} on {ActionName} with {RequestCount} requests",
                ipAddress,
                actionName,
                requestCount);

            context.Result = new RedirectToActionResult("RateLimitExceeded", "Home", routeValues);
            return;
        }

        await next();
    }
}
