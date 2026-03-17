// <copyright file="SessionParticipant.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Domain.Entities;

using System;

public class SessionParticipant
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public SurveySession Session { get; set; } = null!;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime JoinedAt { get; set; }

    public ICollection<Response> Responses { get; set; } = new List<Response>();
}