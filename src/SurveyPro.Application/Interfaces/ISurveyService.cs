// <copyright file="ISurveyService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurveyPro.Domain.Entities;

public interface ISurveyService
{
    Task<Guid> CreateSurveyAsync(string title, string? description, Guid authorId);

    Task<Survey?> GetSurveyAsync(Guid id);

    Task AddQuestionAsync(Guid surveyId, string text, string type, int order);

    Task DeleteSurveyAsync(Guid surveyId);
}
