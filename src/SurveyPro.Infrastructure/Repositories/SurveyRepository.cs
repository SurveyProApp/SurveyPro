// <copyright file="SurveyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SurveyPro.Domain.Entities;
using SurveyPro.Infrastructure.Interfaces;
using SurveyPro.Infrastructure.Persistence;

public class SurveyRepository : ISurveyRepository
{
    private readonly SurveyProDbContext context;

    public SurveyRepository(SurveyProDbContext context)
    {
        this.context = context;
    }

    public async Task<Survey?> GetByIdAsync(Guid id)
    {
        return await context.Surveys
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Survey?> GetWithQuestionsAsync(Guid id)
    {
        return await context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Survey survey)
    {
        await context.Surveys.AddAsync(survey);
    }

    public Task DeleteAsync(Survey survey)
    {
        context.Surveys.Remove(survey);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
