// <copyright file="QuestionServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Application.Services;
using SurveyPro.Domain.Entities;
using SurveyPro.Infrastructure.Interfaces;
using Xunit;

/// <summary>
/// Unit tests for <see cref="QuestionService"/>.
/// </summary>
public class QuestionServiceTests
{
    private static readonly Guid ValidAuthorId = Guid.NewGuid();
    private static readonly Guid ValidSurveyId = Guid.NewGuid();
    private static readonly Guid ValidQuestionId = Guid.NewGuid();

    private static QuestionService BuildService(Mock<IQuestionRepository> repo)
    {
        var logger = new Mock<ILogger<QuestionService>>();
        return new QuestionService(repo.Object, logger.Object);
    }

    private static Survey MakeSurvey(Guid? authorId = null, List<Question>? questions = null) => new Survey
    {
        Id = ValidSurveyId,
        AuthorId = authorId ?? ValidAuthorId,
        Title = "Test Survey",
        Questions = questions ?? new List<Question>(),
    };

    private static Question MakeQuestion(Guid? id = null, Guid? surveyId = null) => new Question
    {
        Id = id ?? ValidQuestionId,
        SurveyId = surveyId ?? ValidSurveyId,
        Text = "Test Question",
        Type = "Text",
        OrderNumber = 1,
    };

    // =====================================================================
    // CreateAsync
    // =====================================================================

    [Fact]
    public async Task CreateAsync_EmptyAuthorId_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        var result = await service.CreateAsync(Guid.Empty, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid author id");
    }

    [Fact]
    public async Task CreateAsync_EmptyText_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "   ", Type = "Text" };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question text is required");
    }

    [Fact]
    public async Task CreateAsync_SurveyNotFound_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Survey not found");
    }

    [Fact]
    public async Task CreateAsync_WrongAuthor_ReturnsFailure()
    {
        // Arrange
        var survey = MakeSurvey(authorId: Guid.NewGuid());
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Access denied");
    }

    [Fact]
    public async Task CreateAsync_SingleChoiceWithLessThanTwoOptions_ReturnsFailure()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "SingleChoice",
            Options = new List<string> { "Only one" },
        };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("At least 2 options are required");
    }

    [Fact]
    public async Task CreateAsync_MultipleChoiceWithNoOptions_ReturnsFailure()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "MultipleChoice",
            Options = null,
        };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("At least 2 options are required");
    }

    [Fact]
    public async Task CreateAsync_ValidTextQuestion_ReturnsSuccessWithNewId()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        var result = await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_ValidTextQuestion_TrimsText()
    {
        // Arrange
        Question? saved = null;
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);
        repo.Setup(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, _) => saved = q)
            .Returns(Task.CompletedTask);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "  My Question  ", Type = "Text" };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        saved!.Text.Should().Be("My Question");
    }

    [Fact]
    public async Task CreateAsync_TextType_SetsOptionsToNull()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
            Options = new List<string> { "A", "B" },
        };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddOptionsAsync(It.IsAny<IEnumerable<AnswerOption>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_EmptySurvey_AssignsOrderNumberOne()
    {
        // Arrange
        Question? saved = null;
        var survey = MakeSurvey(questions: new List<Question>());
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);
        repo.Setup(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, _) => saved = q)
            .Returns(Task.CompletedTask);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        saved!.OrderNumber.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_SurveyWithExistingQuestions_AssignsNextOrderNumber()
    {
        // Arrange
        Question? saved = null;
        var existingQuestions = new List<Question>
        {
            new Question { Id = Guid.NewGuid(), OrderNumber = 1 },
            new Question { Id = Guid.NewGuid(), OrderNumber = 2 },
        };
        var survey = MakeSurvey(questions: existingQuestions);
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);
        repo.Setup(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, _) => saved = q)
            .Returns(Task.CompletedTask);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        saved!.OrderNumber.Should().Be(3);
    }

    [Fact]
    public async Task CreateAsync_ValidSingleChoiceQuestion_CallsAddOptionsOnce()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "SingleChoice",
            Options = new List<string> { "Yes", "No" },
        };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddOptionsAsync(It.IsAny<IEnumerable<AnswerOption>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsRepositoryAddOnce()
    {
        // Arrange
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new CreateQuestionRequestDto { SurveyId = ValidSurveyId, Text = "Q?", Type = "Text" };

        // Act
        await service.CreateAsync(ValidAuthorId, request, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // =====================================================================
    // UpdateAsync
    // =====================================================================

    [Fact]
    public async Task UpdateAsync_EmptyText_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "  ", Type = "Text" };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question text is required");
    }

    [Fact]
    public async Task UpdateAsync_QuestionNotFound_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "Q?", Type = "Text" };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question not found");
    }

    [Fact]
    public async Task UpdateAsync_SurveyNotFound_ReturnsFailure()
    {
        // Arrange
        var question = MakeQuestion();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "Q?", Type = "Text" };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Survey not found");
    }

    [Fact]
    public async Task UpdateAsync_WrongAuthor_ReturnsFailure()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey(authorId: Guid.NewGuid());
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "Q?", Type = "Text" };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Access denied");
    }

    [Fact]
    public async Task UpdateAsync_SingleChoiceWithLessThanTwoOptions_ReturnsFailure()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto
        {
            Text = "Q?",
            Type = "SingleChoice",
            Options = new List<string> { "Only one" },
        };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("At least 2 options are required");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "Updated Q?", Type = "Text" };

        // Act
        var result = await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesQuestionFields()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "  New Text  ", Type = "SingleChoice", Options = new List<string> { "A", "B" } };

        // Act
        await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        question.Text.Should().Be("New Text");
        question.Type.Should().Be("SingleChoice");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_CallsRemoveOptionsOnce()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto { Text = "Q?", Type = "Text" };

        // Act
        await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        repo.Verify(r => r.RemoveOptionsAsync(ValidQuestionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TextType_DoesNotAddOptions()
    {
        // Arrange
        var question = MakeQuestion();
        var survey = MakeSurvey();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetSurveyByIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var service = BuildService(repo);
        var request = new UpdateQuestionRequestDto
        {
            Text = "Q?",
            Type = "Text",
            Options = new List<string> { "A", "B" },
        };

        // Act
        await service.UpdateAsync(ValidQuestionId, ValidAuthorId, request, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddOptionsAsync(It.IsAny<IEnumerable<AnswerOption>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // =====================================================================
    // GetByIdAsync
    // =====================================================================

    [Fact]
    public async Task GetByIdAsync_QuestionNotFound_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        var service = BuildService(repo);

        // Act
        var result = await service.GetByIdAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question not found");
    }

    [Fact]
    public async Task GetByIdAsync_ValidRequest_ReturnsCorrectDto()
    {
        // Arrange
        var question = MakeQuestion();
        question.Options = new List<AnswerOption>
        {
            new AnswerOption { Id = Guid.NewGuid(), Text = "Option A" },
            new AnswerOption { Id = Guid.NewGuid(), Text = "Option B" },
        };

        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        var service = BuildService(repo);

        // Act
        var result = await service.GetByIdAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(ValidQuestionId);
        result.Value.Text.Should().Be("Test Question");
        result.Value.Type.Should().Be("Text");
        result.Value.OrderNumber.Should().Be(1);
        result.Value.Options.Should().HaveCount(2);
    }

    // =====================================================================
    // GetBySurveyIdAsync
    // =====================================================================

    [Fact]
    public async Task GetBySurveyIdAsync_ReturnsAllQuestionsForSurvey()
    {
        // Arrange
        var questions = new List<Question>
        {
            MakeQuestion(Guid.NewGuid()),
            MakeQuestion(Guid.NewGuid()),
            MakeQuestion(Guid.NewGuid()),
        };

        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        var service = BuildService(repo);

        // Act
        var result = await service.GetBySurveyIdAsync(ValidSurveyId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetBySurveyIdAsync_EmptyList_ReturnsEmptyCollection()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        var service = BuildService(repo);

        // Act
        var result = await service.GetBySurveyIdAsync(ValidSurveyId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySurveyIdAsync_MapsOptionsCorrectly()
    {
        // Arrange
        var question = MakeQuestion();
        question.Options = new List<AnswerOption>
        {
            new AnswerOption { Id = Guid.NewGuid(), Text = "Opt1" },
            new AnswerOption { Id = Guid.NewGuid(), Text = "Opt2" },
        };

        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question> { question });

        var service = BuildService(repo);

        // Act
        var result = await service.GetBySurveyIdAsync(ValidSurveyId, CancellationToken.None);

        // Assert
        result.First().Options.Should().BeEquivalentTo(new[] { "Opt1", "Opt2" });
    }

    // =====================================================================
    // DeleteAsync
    // =====================================================================

    [Fact]
    public async Task DeleteAsync_QuestionNotFound_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        var service = BuildService(repo);

        // Act
        var result = await service.DeleteAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question not found");
    }

    [Fact]
    public async Task DeleteAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var question = MakeQuestion();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        var service = BuildService(repo);

        // Act
        var result = await service.DeleteAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ValidRequest_CallsRepositoryRemoveOnce()
    {
        // Arrange
        var question = MakeQuestion();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        var service = BuildService(repo);

        // Act
        await service.DeleteAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        repo.Verify(r => r.Remove(question), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidRequest_ReordersRemainingQuestions()
    {
        // Arrange
        var question = MakeQuestion(id: ValidQuestionId);
        var remaining = new List<Question>
        {
            new Question { Id = Guid.NewGuid(), SurveyId = ValidSurveyId, OrderNumber = 2 },
            new Question { Id = Guid.NewGuid(), SurveyId = ValidSurveyId, OrderNumber = 3 },
        };

        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(remaining);

        var service = BuildService(repo);

        // Act
        await service.DeleteAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        remaining[0].OrderNumber.Should().Be(1);
        remaining[1].OrderNumber.Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_ValidRequest_CallsSaveChangesTwice()
    {
        // Arrange
        var question = MakeQuestion();
        var repo = new Mock<IQuestionRepository>();
        repo.Setup(r => r.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        repo.Setup(r => r.GetQuestionsBySurveyIdAsync(ValidSurveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        var service = BuildService(repo);

        // Act
        await service.DeleteAsync(ValidQuestionId, CancellationToken.None);

        // Assert
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}