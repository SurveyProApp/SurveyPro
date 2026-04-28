// <copyright file="QuoteOfTheDayDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.ExternalApis;

/// <summary>
/// Represents a short inspirational quote fetched from an external API.
/// </summary>
public sealed class QuoteOfTheDayDto
{
    public string Content { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
}