// <copyright file="SurveyParticipationServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SurveyPro.Application.DTOs.Participation;
using SurveyPro.Application.Services;
using SurveyPro.Domain.Entities;
using SurveyPro.Domain.Enums;
using SurveyPro.Infrastructure.Persistence;
using Xunit;

/// <summary>
/// Unit tests for <see cref="SurveyParticipationService"/>.
/// </summary>
public class SurveyParticipationServiceTests
{
    private static readonly Guid ValidSurveyId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidCode = "ABCD1234";
    private const string SurveyNotPublishedMessage = "This survey is being configured and is not available yet.";

    /// <summary>
    /// Creates an in-memory database context for testing.
    /// </summary>
    private static SurveyProDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SurveyProDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SurveyProDbContext(options);
    }

    /// <summary>
    /// Creates a mock logger for testing.
    /// </summary>
    private static ILogger<SurveyParticipationService> CreateMockLogger()
    {
        var mock = new Mock<ILogger<SurveyParticipationService>>();
        return mock.Object;
    }

    private static async Task SeedSessionAsync(
        SurveyProDbContext dbContext,
        SurveyStatuses status,
        string accessCode = ValidCode)
    {
        var survey = new Survey
        {
            Id = ValidSurveyId,
            Title = "Test Survey",
            Status = status,
            IsPublic = true,
            AuthorId = Guid.NewGuid(),
        };

        var session = new SurveySession
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Survey = survey,
            AccessCode = accessCode,
            IsActive = true,
        };

        await dbContext.Surveys.AddAsync(survey);
        await dbContext.SurveySessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByCodeAsync_NullCode_ReturnsFailure()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.GetByCodeAsync(null!, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCodeAsync_InvalidCode_ReturnsFailure()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.GetByCodeAsync("INVALID", null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Survey not found");
    }

    [Fact]
    public async Task GetByCodeAsync_UnpublishedSurvey_ReturnsConfiguredMessage()
    {
        var dbContext = CreateDbContext();
        await SeedSessionAsync(dbContext, SurveyStatuses.Draft);

        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.GetByCodeAsync(ValidCode, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SurveyNotPublishedMessage);
    }

    [Fact]
    public async Task SaveDraftAsync_InvalidUserId_ReturnsFailure()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var request = new SaveDraftRequestDto
        {
            SurveyId = ValidSurveyId,
            AccessCode = ValidCode,
            Answers = new List<ParticipationAnswerDto>(),
        };

        var result = await service.SaveDraftAsync(Guid.Empty, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SaveDraftAsync_NullCode_ReturnsFailure()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var request = new SaveDraftRequestDto
        {
            SurveyId = ValidSurveyId,
            AccessCode = null!,
            Answers = new List<ParticipationAnswerDto>(),
        };

        var result = await service.SaveDraftAsync(ValidUserId, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SaveDraftAsync_UnpublishedSurvey_ReturnsConfiguredMessage()
    {
        var dbContext = CreateDbContext();
        await SeedSessionAsync(dbContext, SurveyStatuses.Draft);

        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var request = new SaveDraftRequestDto
        {
            SurveyId = ValidSurveyId,
            AccessCode = ValidCode,
            Answers = new List<ParticipationAnswerDto>(),
        };

        var result = await service.SaveDraftAsync(ValidUserId, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SurveyNotPublishedMessage);
    }

    [Fact]
    public async Task ClearDraftAsync_InvalidSession_ReturnsFailureWithoutThrowing()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.ClearDraftAsync(ValidUserId, "INVALID", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitAsync_InvalidSession_ReturnsFailure()
    {
        var dbContext = CreateDbContext();
        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.SubmitAsync(ValidUserId, "INVALID", ValidSurveyId, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Survey not found");
    }

    [Fact]
    public async Task SubmitAsync_UnpublishedSurvey_ReturnsConfiguredMessage()
    {
        var dbContext = CreateDbContext();
        await SeedSessionAsync(dbContext, SurveyStatuses.Draft);

        var logger = CreateMockLogger();
        var service = new SurveyParticipationService(dbContext, logger);

        var result = await service.SubmitAsync(ValidUserId, ValidCode, ValidSurveyId, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SurveyNotPublishedMessage);
    }
}
