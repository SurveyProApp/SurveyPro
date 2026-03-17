// <copyright file="AnswerOption.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class AnswerOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public ICollection<ResponseAnswer> ResponseAnswers { get; set; } = new List<ResponseAnswer>();
}