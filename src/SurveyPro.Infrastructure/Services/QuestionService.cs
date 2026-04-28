// <copyright file="QuestionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using SurveyPro.Application.Common;
using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Application.Interfaces;
using SurveyPro.Domain.Entities;
using SurveyPro.Infrastructure.Interfaces;

/// <summary>
/// Question use-case service.
/// </summary>
public sealed class QuestionService : IQuestionService
{
    private readonly IQuestionRepository repository;
    private readonly ILogger<QuestionService> logger;

    public QuestionService(IQuestionRepository repository, ILogger<QuestionService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<Guid>> CreateAsync(
        Guid authorId,
        CreateQuestionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return "Question text is required";
        }

        if (authorId == Guid.Empty)
        {
            return "Invalid author id";
        }

        var survey = await this.repository.GetSurveyByIdAsync(request.SurveyId, cancellationToken);

        if (survey == null)
        {
            return "Survey not found";
        }

        if (survey.AuthorId != authorId)
        {
            return "Access denied";
        }

        if (request.Type == "Text")
        {
            request.Options = null;
        }

        if ((request.Type == "SingleChoice" || request.Type == "MultipleChoice")
            && (request.Options == null || request.Options.Count < 2))
        {
            return "At least 2 options are required";
        }

        var order = (survey.Questions?.Any() == true)
            ? survey.Questions.Max(q => q.OrderNumber) + 1
            : 1;

        var question = new Question
        {
            Id = Guid.NewGuid(),
            SurveyId = request.SurveyId,
            Text = request.Text.Trim(),
            Type = request.Type,
            OrderNumber = order,
        };

        await this.repository.AddAsync(question, cancellationToken);

        if (request.Options != null && request.Options.Any())
        {
            var options = request.Options.Select(o => new AnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = o,
            });

            await this.repository.AddOptionsAsync(options, cancellationToken);
        }

        await this.repository.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation("Question {QuestionId} created", question.Id);

        return question.Id;
    }

    public async Task<Result> UpdateAsync(
        Guid questionId,
        Guid authorId,
        UpdateQuestionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return "Question text is required";
        }

        var question = await this.repository.GetByIdAsync(questionId, cancellationToken);

        if (question == null)
        {
            return "Question not found";
        }

        var survey = await this.repository.GetSurveyByIdAsync(question.SurveyId, cancellationToken);

        if (survey == null)
        {
            return "Survey not found";
        }

        if (survey.AuthorId != authorId)
        {
            return "Access denied";
        }

        if (request.Type == "Text")
        {
            request.Options = null;
        }

        if ((request.Type == "SingleChoice" || request.Type == "MultipleChoice")
            && (request.Options == null || request.Options.Count < 2))
        {
            return "At least 2 options are required";
        }

        question.Text = request.Text.Trim();
        question.Type = request.Type;

        await this.repository.RemoveOptionsAsync(questionId, cancellationToken);

        if (request.Options != null && request.Options.Any())
        {
            var options = request.Options.Select(o => new AnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = o,
            });

            await this.repository.AddOptionsAsync(options, cancellationToken);
        }

        await this.repository.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation("Question {QuestionId} updated", question.Id);

        return Result.Success();
    }

    public async Task<List<QuestionDto>> GetBySurveyIdAsync(Guid surveyId, CancellationToken cancellationToken)
    {
        var questions = await this.repository.GetQuestionsBySurveyIdAsync(surveyId, cancellationToken);

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            Type = q.Type,
            OrderNumber = q.OrderNumber,
            Options = q.Options?.Select(o => o.Text).ToList(),
        }).ToList();
    }

    public async Task<Result<QuestionDto>> GetByIdAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await this.repository.GetByIdAsync(questionId, cancellationToken);

        if (question == null)
        {
            return "Question not found";
        }

        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            Type = question.Type,
            OrderNumber = question.OrderNumber,
            Options = question.Options?.Select(o => o.Text).ToList(),
        };
    }

    public async Task<Result> DeleteAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await this.repository.GetByIdAsync(questionId, cancellationToken);

        if (question == null)
        {
            return Result.Failure("Question not found");
        }

        var surveyId = question.SurveyId;

        this.repository.Remove(question);
        await this.repository.SaveChangesAsync(cancellationToken);

        var questions = await this.repository.GetQuestionsBySurveyIdAsync(surveyId, cancellationToken);

        var ordered = questions
            .OrderBy(q => q.OrderNumber)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].OrderNumber = i + 1;
        }

        await this.repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
