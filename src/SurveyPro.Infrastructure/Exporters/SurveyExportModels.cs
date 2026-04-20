// <copyright file="SurveyExportModels.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Exporters;

public sealed class SurveyResponsesExportModel
{
    public Guid SurveyId { get; set; }

    public string SurveyTitle { get; set; } = string.Empty;

    public string? SurveyDescription { get; set; }

    public string AccessCode { get; set; } = string.Empty;

    public int TotalSubmittedResponses { get; set; }

    public IReadOnlyCollection<SurveyResponseExportModel> Responses { get; set; } = Array.Empty<SurveyResponseExportModel>();
}

public sealed class SurveyResponseExportModel
{
    public Guid ResponseId { get; set; }

    public Guid RespondentUserId { get; set; }

    public string RespondentName { get; set; } = string.Empty;

    public string RespondentEmail { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public IReadOnlyCollection<SurveyResponseAnswerExportModel> Answers { get; set; } = Array.Empty<SurveyResponseAnswerExportModel>();
}

public sealed class SurveyResponseAnswerExportModel
{
    public Guid QuestionId { get; set; }

    public int QuestionOrderNumber { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public string? TextAnswer { get; set; }

    public IReadOnlyCollection<Guid> SelectedOptionIds { get; set; } = Array.Empty<Guid>();

    public IReadOnlyCollection<string> SelectedOptionTexts { get; set; } = Array.Empty<string>();
}