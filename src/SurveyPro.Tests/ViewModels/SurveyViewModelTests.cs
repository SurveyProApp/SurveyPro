// <copyright file="SurveyViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Web.ViewModels.Surveys;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CreateSurveyViewModel"/> and <see cref="EditSurveyViewModel"/>.
/// </summary>
public class SurveyViewModelTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model);
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    // =====================================================================
    // CreateSurveyViewModel
    // =====================================================================

    [Fact]
    public void CreateSurveyViewModel_ValidModel_PassesValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = "My Survey",
            Description = "Some description",
            IsPublic = true,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateSurveyViewModel_EmptyTitle_FailsValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = string.Empty,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateSurveyViewModel.Title)));
    }

    [Fact]
    public void CreateSurveyViewModel_TitleExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = new string('A', 201),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateSurveyViewModel.Title)));
    }

    [Fact]
    public void CreateSurveyViewModel_TitleAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = new string('A', 200),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateSurveyViewModel_DescriptionExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = "Title",
            Description = new string('A', 2001),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateSurveyViewModel.Description)));
    }

    [Fact]
    public void CreateSurveyViewModel_DescriptionAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = "Title",
            Description = new string('A', 2000),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateSurveyViewModel_NullDescription_PassesValidation()
    {
        // Arrange
        var model = new CreateSurveyViewModel
        {
            Title = "Title",
            Description = null,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateSurveyViewModel_DefaultIsPublic_IsFalse()
    {
        // Arrange & Act
        var model = new CreateSurveyViewModel();

        // Assert
        model.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void CreateSurveyViewModel_DefaultTitle_IsEmptyString()
    {
        // Arrange & Act
        var model = new CreateSurveyViewModel();

        // Assert
        model.Title.Should().BeEmpty();
    }

    // =====================================================================
    // EditSurveyViewModel
    // =====================================================================

    [Fact]
    public void EditSurveyViewModel_ValidModel_PassesValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = "My Survey",
            Description = "Some description",
            IsPublic = false,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_EmptyTitle_FailsValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = string.Empty,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditSurveyViewModel.Title)));
    }

    [Fact]
    public void EditSurveyViewModel_TitleExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 201),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditSurveyViewModel.Title)));
    }

    [Fact]
    public void EditSurveyViewModel_TitleAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 200),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_DescriptionExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = new string('A', 2001),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditSurveyViewModel.Description)));
    }

    [Fact]
    public void EditSurveyViewModel_DescriptionAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = new string('A', 2000),
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_NullDescription_PassesValidation()
    {
        // Arrange
        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = null,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_DefaultQuestions_IsEmptyList()
    {
        // Arrange & Act
        var model = new EditSurveyViewModel();

        // Assert
        model.Questions.Should().NotBeNull();
        model.Questions.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_DefaultTitle_IsEmptyString()
    {
        // Arrange & Act
        var model = new EditSurveyViewModel();

        // Assert
        model.Title.Should().BeEmpty();
    }

    [Fact]
    public void EditSurveyViewModel_WithQuestions_StoresQuestionsCorrectly()
    {
        // Arrange
        var questions = new List<QuestionDto>
        {
            new QuestionDto { Id = Guid.NewGuid(), Text = "Q1", Type = "Text", OrderNumber = 1 },
            new QuestionDto { Id = Guid.NewGuid(), Text = "Q2", Type = "SingleChoice", OrderNumber = 2 },
        };

        var model = new EditSurveyViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Questions = questions,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
        model.Questions.Should().HaveCount(2);
    }
}