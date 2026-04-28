// <copyright file="AdminSurveyQuestionsDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.Surveys;

using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Domain.Enums;

/// <summary>
/// Survey questions payload for admin read-only view.
/// </summary>
public sealed class AdminSurveyQuestionsDto
{
    /// <summary>
    /// Gets or sets survey identifier.
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Gets or sets survey title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets survey description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets survey status.
    /// </summary>
    public SurveyStatuses Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether survey is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets active access code.
    /// </summary>
    public string AccessCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets author name.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets author email.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets questions.
    /// </summary>
    public IReadOnlyCollection<QuestionDto> Questions { get; set; } = Array.Empty<QuestionDto>();
}