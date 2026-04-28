// <copyright file="IQuoteService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.Interfaces;

using SurveyPro.Application.DTOs.ExternalApis;

/// <summary>
/// Provides access to a small external quote API.
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Gets an inspirational quote from an external API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Quote data or null when the API is unavailable.</returns>
    Task<QuoteOfTheDayDto?> GetQuoteAsync(CancellationToken cancellationToken);
}