// <copyright file="SurveyService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurveyPro.Application.Interfaces;
using SurveyPro.Domain.Entities;
using SurveyPro.Infrastructure.Interfaces;

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository surveyRepository;

    public SurveyService(ISurveyRepository surveyRepository)
    {
        this.surveyRepository = surveyRepository;
    }

    public async Task<Guid> CreateSurveyAsync(string title, string? description, Guid authorId)
    {
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            AuthorId = authorId,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
        };

        await surveyRepository.AddAsync(survey);
        await surveyRepository.SaveChangesAsync();

        return survey.Id;
    }

    public async Task<Survey?> GetSurveyAsync(Guid id)
    {
        return await surveyRepository.GetWithQuestionsAsync(id);
    }

    public async Task AddQuestionAsync(Guid surveyId, string text, string type, int order)
    {
        var survey = await surveyRepository.GetWithQuestionsAsync(surveyId);

        if (survey == null)
        {
            throw new Exception("Survey not found");
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            Text = text,
            Type = type,
            OrderNumber = order,
        };

        survey.Questions.Add(question);

        await surveyRepository.SaveChangesAsync();
    }

    public async Task DeleteSurveyAsync(Guid surveyId)
    {
        var survey = await surveyRepository.GetByIdAsync(surveyId);

        if (survey == null)
        {
            throw new Exception("Survey not found");
        }

        await surveyRepository.DeleteAsync(survey);
        await surveyRepository.SaveChangesAsync();
    }
}
