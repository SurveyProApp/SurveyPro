// <copyright file="AdminSurveyServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SurveyPro.Application.Services;
using SurveyPro.Domain.Entities;
using SurveyPro.Domain.Enums;
using SurveyPro.Infrastructure.Persistence;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AdminSurveyService"/>.
/// </summary>
public class AdminSurveyServiceTests
{
    private static SurveyProDbContext CreateDbContext() =>
        new SurveyProDbContext(
            new DbContextOptionsBuilder<SurveyProDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    private static AdminSurveyService BuildService(SurveyProDbContext dbContext)
    {
        var logger = new Mock<ILogger<AdminSurveyService>>();
        return new AdminSurveyService(dbContext, logger.Object);
    }

    private static async Task<(Survey survey, SurveySession session)> SeedPublishedSurveyAsync(
        SurveyProDbContext db,
        string accessCode = "TESTCODE",
        int questionCount = 2)
    {
        var author = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = "Author",
            Email = "author@test.com",
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = "Published Survey",
            Description = "Desc",
            Status = SurveyStatuses.Published,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            AuthorId = author.Id,
            Author = author,
        };

        var questions = Enumerable.Range(1, questionCount).Select(i => new Question
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Text = $"Q{i}",
            Type = "Text",
            OrderNumber = i,
        }).ToList();

        survey.Questions = questions;

        var session = new SurveySession
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            AccessCode = accessCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await db.Users.AddAsync(author);
        await db.Surveys.AddAsync(survey);
        await db.SurveySessions.AddAsync(session);
        await db.SaveChangesAsync();

        return (survey, session);
    }

    [Fact]
    public async Task GetAllSurveysAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var db = CreateDbContext();
        var service = BuildService(db);

        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSurveysAsync_ExcludesDraftSurveys()
    {
        var db = CreateDbContext();

        var author = new ApplicationUser { Id = Guid.NewGuid(), Name = "A", Email = "a@a.com" };
        var draft = new Survey
        {
            Id = Guid.NewGuid(),
            Title = "Draft",
            Status = SurveyStatuses.Draft,
            AuthorId = author.Id,
            Author = author,
        };

        await db.Users.AddAsync(author);
        await db.Surveys.AddAsync(draft);
        await db.SaveChangesAsync();

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSurveysAsync_PublishedSurvey_IsIncluded()
    {
        var db = CreateDbContext();
        await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllSurveysAsync_MapsAuthorNameAndEmail()
    {
        var db = CreateDbContext();
        await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);
        var dto = result.First();

        dto.AuthorName.Should().Be("Author");
        dto.AuthorEmail.Should().Be("author@test.com");
    }

    [Fact]
    public async Task GetAllSurveysAsync_MapsAccessCode()
    {
        var db = CreateDbContext();
        await SeedPublishedSurveyAsync(db, accessCode: "MYCODE01");

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.First().AccessCode.Should().Be("MYCODE01");
    }

    [Fact]
    public async Task GetAllSurveysAsync_MapsQuestionCount()
    {
        var db = CreateDbContext();
        await SeedPublishedSurveyAsync(db, questionCount: 3);

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.First().QuestionCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllSurveysAsync_NoActiveSession_ReturnsEmptyAccessCode()
    {
        var db = CreateDbContext();

        var author = new ApplicationUser { Id = Guid.NewGuid(), Name = "A", Email = "a@a.com" };
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = "No Session Survey",
            Status = SurveyStatuses.Published,
            AuthorId = author.Id,
            Author = author,
        };

        // Session explicitly inactive
        var session = new SurveySession
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            AccessCode = "INACTIVE",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
        };

        await db.Users.AddAsync(author);
        await db.Surveys.AddAsync(survey);
        await db.SurveySessions.AddAsync(session);
        await db.SaveChangesAsync();

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.First().AccessCode.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSurveysAsync_OrdersByCreatedAtDescending()
    {
        var db = CreateDbContext();
        var author = new ApplicationUser { Id = Guid.NewGuid(), Name = "A", Email = "a@a.com" };
        await db.Users.AddAsync(author);

        var older = new Survey
        {
            Id = Guid.NewGuid(),
            Title = "Older",
            Status = SurveyStatuses.Published,
            AuthorId = author.Id,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
        };
        var newer = new Survey
        {
            Id = Guid.NewGuid(),
            Title = "Newer",
            Status = SurveyStatuses.Published,
            AuthorId = author.Id,
            Author = author,
            CreatedAt = DateTime.UtcNow,
        };

        await db.Surveys.AddRangeAsync(older, newer);
        await db.SaveChangesAsync();

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.First().Title.Should().Be("Newer");
    }

    [Fact]
    public async Task GetAllSurveysAsync_ResponseCountIsZeroWhenNoSubmissions()
    {
        var db = CreateDbContext();
        await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        var result = await service.GetAllSurveysAsync(CancellationToken.None);

        result.First().ResponseCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteSurveyAsync_SurveyNotFound_ReturnsFalse()
    {
        var db = CreateDbContext();
        var service = BuildService(db);

        var result = await service.DeleteSurveyAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSurveyAsync_ValidId_ReturnsTrue()
    {
        var db = CreateDbContext();
        var (survey, _) = await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        var result = await service.DeleteSurveyAsync(survey.Id, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSurveyAsync_ValidId_RemovesSurveyFromDatabase()
    {
        var db = CreateDbContext();
        var (survey, _) = await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        await service.DeleteSurveyAsync(survey.Id, CancellationToken.None);

        var remaining = await db.Surveys.FindAsync(survey.Id);
        remaining.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSurveyAsync_CalledTwiceWithSameId_SecondCallReturnsFalse()
    {
        var db = CreateDbContext();
        var (survey, _) = await SeedPublishedSurveyAsync(db);

        var service = BuildService(db);
        await service.DeleteSurveyAsync(survey.Id, CancellationToken.None);
        var second = await service.DeleteSurveyAsync(survey.Id, CancellationToken.None);

        second.Should().BeFalse();
    }
}