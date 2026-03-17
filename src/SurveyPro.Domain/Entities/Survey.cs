// <copyright file="Survey.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class Survey
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid AuthorId { get; set; }

    public ApplicationUser Author { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<SurveySession> Sessions { get; set; } = new List<SurveySession>();
}