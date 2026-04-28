// <copyright file="QuoteService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.ExternalApis;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SurveyPro.Application.DTOs.ExternalApis;
using SurveyPro.Application.Interfaces;

/// <summary>
/// Typed HTTP client adapter for the Quotable API living in Infrastructure.
/// Implements the application contract `IQuoteService`.
/// </summary>
public sealed class QuoteService : IQuoteService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<QuoteService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteService"/> class.
    /// </summary>
    /// <param name="httpClient">Configured HTTP client.</param>
    /// <param name="logger">Logger instance.</param>
    public QuoteService(HttpClient httpClient, ILogger<QuoteService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<QuoteOfTheDayDto?> GetQuoteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await this.httpClient.GetFromJsonAsync<QuoteApiResponse>("random", cancellationToken);

            if (response is null || string.IsNullOrWhiteSpace(response.Content))
            {
                return null;
            }

            return new QuoteOfTheDayDto
            {
                Content = response.Content,
                Author = response.Author ?? "Unknown",
                Source = "quotable.io",
            };
        }
        catch (Exception exception)
        {
            this.logger.LogWarning(exception, "Failed to load quote from external API");
            return null;
        }
    }

    private sealed class QuoteApiResponse
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }
    }
}
