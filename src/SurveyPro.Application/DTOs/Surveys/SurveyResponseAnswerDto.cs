// <copyright file="SurveyResponseAnswerDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.Surveys;

/// <summary>
/// One answer inside a respondent submission.
/// </summary>
public sealed class SurveyResponseAnswerDto
{
    public Guid QuestionId { get; set; }

    public int QuestionOrderNumber { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public string? TextAnswer { get; set; }

    public IReadOnlyCollection<Guid> SelectedOptionIds { get; set; } = Array.Empty<Guid>();

    public IReadOnlyCollection<string> SelectedOptionTexts { get; set; } = Array.Empty<string>();
}
