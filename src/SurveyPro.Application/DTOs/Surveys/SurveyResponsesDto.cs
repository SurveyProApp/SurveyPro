// <copyright file="SurveyResponsesDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.Surveys;

/// <summary>
/// Full survey responses payload for authors and administrators.
/// </summary>
public sealed class SurveyResponsesDto
{
    public Guid SurveyId { get; set; }

    public string SurveyTitle { get; set; } = string.Empty;

    public string? SurveyDescription { get; set; }

    public string AccessCode { get; set; } = string.Empty;

    public int TotalSubmittedResponses { get; set; }

    public IReadOnlyCollection<SurveyResponseDto> Responses { get; set; } = Array.Empty<SurveyResponseDto>();
}
