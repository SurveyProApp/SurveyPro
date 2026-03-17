// <copyright file="Question.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class Question
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int OrderNumber { get; set; }
}