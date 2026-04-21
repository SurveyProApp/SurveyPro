// <copyright file="CacheSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.Configuration;

/// <summary>
/// Application cache settings loaded from configuration.
/// </summary>
public sealed class CacheSettings
{
    /// <summary>
    /// Gets or sets users list cache lifetime in minutes.
    /// </summary>
    public int UsersListExpirationMinutes { get; set; } = 10;
}