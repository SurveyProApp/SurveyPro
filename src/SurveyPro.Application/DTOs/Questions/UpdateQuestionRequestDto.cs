// <copyright file="UpdateQuestionRequestDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.DTOs.Questions;

public sealed class UpdateQuestionRequestDto
{
    public string Text { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public List<string>? Options { get; set; }
}