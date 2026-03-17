// <copyright file="ISurveyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurveyPro.Domain.Entities;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(Guid id);

    Task<Survey?> GetWithQuestionsAsync(Guid id);

    Task AddAsync(Survey survey);

    Task DeleteAsync(Survey survey);

    Task SaveChangesAsync();
}
