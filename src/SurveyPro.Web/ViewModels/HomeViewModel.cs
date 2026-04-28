// <copyright file="HomeViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.ViewModels;

using SurveyPro.Application.DTOs.ExternalApis;

/// <summary>
/// Home page view model.
/// </summary>
public sealed class HomeViewModel
{
    /// <summary>
    /// Gets or sets external quote shown on the home page.
    /// </summary>
    public QuoteOfTheDayDto? Quote { get; set; }
}
