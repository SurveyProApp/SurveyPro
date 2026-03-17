// <copyright file="ResponseAnswer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class ResponseAnswer
{
    public Guid Id { get; set; }

    public Guid ResponseId { get; set; }

    public Guid? OptionId { get; set; }

    public string? TextAnswer { get; set; }
}