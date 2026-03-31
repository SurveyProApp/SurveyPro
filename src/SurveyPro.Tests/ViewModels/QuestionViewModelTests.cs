// <copyright file="QuestionViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SurveyPro.Web.ViewModels.Questions;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CreateQuestionViewModel"/> and <see cref="EditQuestionViewModel"/>.
/// </summary>
public class QuestionViewModelTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model);
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    // =====================================================================
    // CreateQuestionViewModel
    // =====================================================================

    [Fact]
    public void CreateQuestionViewModel_ValidModel_PassesValidation()
    {
        // Arrange
        var model = new CreateQuestionViewModel
        {
            SurveyId = Guid.NewGuid(),
            Text = "What is your name?",
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateQuestionViewModel_EmptyText_FailsValidation()
    {
        // Arrange
        var model = new CreateQuestionViewModel
        {
            SurveyId = Guid.NewGuid(),
            Text = string.Empty,
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateQuestionViewModel.Text)));
    }

    [Fact]
    public void CreateQuestionViewModel_TextExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new CreateQuestionViewModel
        {
            SurveyId = Guid.NewGuid(),
            Text = new string('A', 501),
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateQuestionViewModel.Text)));
    }

    [Fact]
    public void CreateQuestionViewModel_TextAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new CreateQuestionViewModel
        {
            SurveyId = Guid.NewGuid(),
            Text = new string('A', 500),
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateQuestionViewModel_EmptyType_FailsValidation()
    {
        // Arrange
        var model = new CreateQuestionViewModel
        {
            SurveyId = Guid.NewGuid(),
            Text = "Q?",
            Type = string.Empty,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateQuestionViewModel.Type)));
    }

    [Fact]
    public void CreateQuestionViewModel_DefaultOptions_IsEmptyList()
    {
        // Arrange & Act
        var model = new CreateQuestionViewModel();

        // Assert
        model.Options.Should().NotBeNull();
        model.Options.Should().BeEmpty();
    }

    [Fact]
    public void CreateQuestionViewModel_DefaultType_IsText()
    {
        // Arrange & Act
        var model = new CreateQuestionViewModel();

        // Assert
        model.Type.Should().Be("Text");
    }

    // =====================================================================
    // EditQuestionViewModel
    // =====================================================================

    [Fact]
    public void EditQuestionViewModel_ValidModel_PassesValidation()
    {
        // Arrange
        var model = new EditQuestionViewModel
        {
            Id = Guid.NewGuid(),
            SurveyId = Guid.NewGuid(),
            Text = "What is your name?",
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditQuestionViewModel_EmptyText_FailsValidation()
    {
        // Arrange
        var model = new EditQuestionViewModel
        {
            Id = Guid.NewGuid(),
            SurveyId = Guid.NewGuid(),
            Text = string.Empty,
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditQuestionViewModel.Text)));
    }

    [Fact]
    public void EditQuestionViewModel_TextExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var model = new EditQuestionViewModel
        {
            Id = Guid.NewGuid(),
            SurveyId = Guid.NewGuid(),
            Text = new string('A', 501),
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditQuestionViewModel.Text)));
    }

    [Fact]
    public void EditQuestionViewModel_TextAtMaxLength_PassesValidation()
    {
        // Arrange
        var model = new EditQuestionViewModel
        {
            Id = Guid.NewGuid(),
            SurveyId = Guid.NewGuid(),
            Text = new string('A', 500),
            Type = "Text",
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void EditQuestionViewModel_EmptyType_FailsValidation()
    {
        // Arrange
        var model = new EditQuestionViewModel
        {
            Id = Guid.NewGuid(),
            SurveyId = Guid.NewGuid(),
            Text = "Q?",
            Type = string.Empty,
        };

        // Act
        var results = Validate(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(EditQuestionViewModel.Type)));
    }

    [Fact]
    public void EditQuestionViewModel_DefaultOptions_IsEmptyList()
    {
        // Arrange & Act
        var model = new EditQuestionViewModel();

        // Assert
        model.Options.Should().NotBeNull();
        model.Options.Should().BeEmpty();
    }

    [Fact]
    public void EditQuestionViewModel_DefaultType_IsText()
    {
        // Arrange & Act
        var model = new EditQuestionViewModel();

        // Assert
        model.Type.Should().Be("Text");
    }
}