// <copyright file="SurveyResponsesViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.ViewModels.Surveys;

/// <summary>
/// View model for survey responses page.
/// </summary>
public sealed class SurveyResponsesViewModel
{
    public Guid SurveyId { get; set; }

    public string SurveyTitle { get; set; } = string.Empty;

    public string? SurveyDescription { get; set; }

    public string AccessCode { get; set; } = string.Empty;

    public int TotalSubmittedResponses { get; set; }

    public List<SurveyResponseViewModel> Responses { get; set; } = new ();
}

public sealed class SurveyResponseViewModel
{
    public Guid ResponseId { get; set; }

    public Guid RespondentUserId { get; set; }

    public string RespondentName { get; set; } = string.Empty;

    public string RespondentEmail { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public List<SurveyResponseAnswerViewModel> Answers { get; set; } = new ();
}

public sealed class SurveyResponseAnswerViewModel
{
    public Guid QuestionId { get; set; }

    public int QuestionOrderNumber { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public string? TextAnswer { get; set; }

    public List<Guid> SelectedOptionIds { get; set; } = new ();

    public List<string> SelectedOptionTexts { get; set; } = new ();
}
