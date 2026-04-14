// <copyright file="SurveyResponseDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.Surveys;

/// <summary>
/// One respondent submission.
/// </summary>
public sealed class SurveyResponseDto
{
    public Guid ResponseId { get; set; }

    public Guid RespondentUserId { get; set; }

    public string RespondentName { get; set; } = string.Empty;

    public string RespondentEmail { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public IReadOnlyCollection<SurveyResponseAnswerDto> Answers { get; set; } = Array.Empty<SurveyResponseAnswerDto>();
}
