// <copyright file="SurveySession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class SurveySession
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }

    public Survey Survey { get; set; } = null!;

    public string AccessCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; }

    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
}