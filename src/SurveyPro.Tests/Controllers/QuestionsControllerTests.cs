// <copyright file="QuestionsControllerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Controllers;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SurveyPro.Application.Common;
using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Application.Interfaces;
using SurveyPro.Web.Controllers;
using SurveyPro.Web.ViewModels.Questions;
using Xunit;

/// <summary>
/// Unit tests for <see cref="QuestionsController"/>.
/// </summary>
public class QuestionsControllerTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSurveyId = Guid.NewGuid();
    private static readonly Guid ValidQuestionId = Guid.NewGuid();

    private static QuestionsController BuildController(
        Mock<IQuestionService> service,
        Guid? userId = null)
    {
        var controller = new QuestionsController(service.Object);

        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        var tempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>());

        controller.TempData = tempData;

        return controller;
    }

    private static QuestionDto MakeQuestionDto() => new QuestionDto
    {
        Id = ValidQuestionId,
        SurveyId = ValidSurveyId,
        Text = "Test Question",
        Type = "Text",
        OrderNumber = 1,
        Options = new List<string>(),
    };

    // =====================================================================
    // Create POST
    // =====================================================================

    [Fact]
    public async Task Create_InvalidModelState_RedirectsToSurveyEdit()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, ValidUserId);
        controller.ModelState.AddModelError("Text", "Required");

        var model = new CreateQuestionViewModel { SurveyId = ValidSurveyId };

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.ControllerName.Should().Be("Surveys");
        redirect.RouteValues!["id"].Should().Be(ValidSurveyId);
    }

    [Fact]
    public async Task Create_NoAuthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, userId: null);

        var model = new CreateQuestionViewModel
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Account");
    }

    [Fact]
    public async Task Create_ServiceReturnsFailure_SetsTempDataErrorMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.CreateAsync(ValidUserId, It.IsAny<CreateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Survey not found"));

        var controller = BuildController(service, ValidUserId);
        var model = new CreateQuestionViewModel
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        await controller.Create(model, CancellationToken.None);

        // Assert
        controller.TempData["ErrorMessage"].Should().Be("Survey not found");
    }

    [Fact]
    public async Task Create_ServiceReturnsSuccess_SetsTempDataSuccessMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.CreateAsync(ValidUserId, It.IsAny<CreateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var controller = BuildController(service, ValidUserId);
        var model = new CreateQuestionViewModel
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        await controller.Create(model, CancellationToken.None);

        // Assert
        controller.TempData["SuccessMessage"].Should().Be("Question added");
    }

    [Fact]
    public async Task Create_ValidRequest_RedirectsToSurveyEdit()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.CreateAsync(ValidUserId, It.IsAny<CreateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var controller = BuildController(service, ValidUserId);
        var model = new CreateQuestionViewModel
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.ControllerName.Should().Be("Surveys");
        redirect.RouteValues!["id"].Should().Be(ValidSurveyId);
    }

    [Fact]
    public async Task Create_ValidRequest_FiltersEmptyOptions()
    {
        // Arrange
        CreateQuestionRequestDto? captured = null;
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.CreateAsync(ValidUserId, It.IsAny<CreateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, CreateQuestionRequestDto, CancellationToken>((_, dto, _) => captured = dto)
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var controller = BuildController(service, ValidUserId);
        var model = new CreateQuestionViewModel
        {
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "SingleChoice",
            Options = new List<string> { "Yes", "", "No", "  " },
        };

        // Act
        await controller.Create(model, CancellationToken.None);

        // Assert
        captured!.Options.Should().BeEquivalentTo(new[] { "Yes", "No" });
    }

    // =====================================================================
    // Edit GET
    // =====================================================================

    [Fact]
    public async Task EditGet_NoAuthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, userId: null);

        // Act
        var result = await controller.Edit(ValidQuestionId, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Account");
    }

    [Fact]
    public async Task EditGet_QuestionNotFound_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<QuestionDto>.Failure("Question not found"));

        var controller = BuildController(service, ValidUserId);

        // Act
        var result = await controller.Edit(ValidQuestionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task EditGet_ValidRequest_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var dto = MakeQuestionDto();
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.GetByIdAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<QuestionDto>.Success(dto));

        var controller = BuildController(service, ValidUserId);

        // Act
        var result = await controller.Edit(ValidQuestionId, CancellationToken.None);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<EditQuestionViewModel>().Subject;
        model.Id.Should().Be(ValidQuestionId);
        model.SurveyId.Should().Be(ValidSurveyId);
        model.Text.Should().Be("Test Question");
        model.Type.Should().Be("Text");
    }

    // =====================================================================
    // Edit POST
    // =====================================================================

    [Fact]
    public async Task EditPost_InvalidModelState_ReturnsView()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, ValidUserId);
        controller.ModelState.AddModelError("Text", "Required");

        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = string.Empty,
            Type = "Text",
        };

        // Act
        var result = await controller.Edit(model, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task EditPost_NoAuthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, userId: null);

        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        var result = await controller.Edit(model, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Account");
    }

    [Fact]
    public async Task EditPost_ServiceReturnsFailure_SetsTempDataErrorMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.UpdateAsync(ValidQuestionId, ValidUserId, It.IsAny<UpdateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Access denied"));

        var controller = BuildController(service, ValidUserId);
        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        await controller.Edit(model, CancellationToken.None);

        // Assert
        controller.TempData["ErrorMessage"].Should().Be("Access denied");
    }

    [Fact]
    public async Task EditPost_ServiceReturnsSuccess_SetsTempDataSuccessMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.UpdateAsync(ValidQuestionId, ValidUserId, It.IsAny<UpdateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);
        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        await controller.Edit(model, CancellationToken.None);

        // Assert
        controller.TempData["SuccessMessage"].Should().Be("Question updated");
    }

    [Fact]
    public async Task EditPost_ValidRequest_RedirectsToSurveyEdit()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.UpdateAsync(ValidQuestionId, ValidUserId, It.IsAny<UpdateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);
        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "Text",
        };

        // Act
        var result = await controller.Edit(model, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.ControllerName.Should().Be("Surveys");
        redirect.RouteValues!["id"].Should().Be(ValidSurveyId);
    }

    [Fact]
    public async Task EditPost_ValidRequest_FiltersEmptyOptions()
    {
        // Arrange
        UpdateQuestionRequestDto? captured = null;
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.UpdateAsync(ValidQuestionId, ValidUserId, It.IsAny<UpdateQuestionRequestDto>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, UpdateQuestionRequestDto, CancellationToken>((_, _, dto, _) => captured = dto)
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);
        var model = new EditQuestionViewModel
        {
            Id = ValidQuestionId,
            SurveyId = ValidSurveyId,
            Text = "Q?",
            Type = "MultipleChoice",
            Options = new List<string> { "A", "  ", "B", "" },
        };

        // Act
        await controller.Edit(model, CancellationToken.None);

        // Assert
        captured!.Options.Should().BeEquivalentTo(new[] { "A", "B" });
    }

    // =====================================================================
    // Delete POST
    // =====================================================================

    [Fact]
    public async Task Delete_NoAuthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        var controller = BuildController(service, userId: null);

        // Act
        var result = await controller.Delete(ValidQuestionId, ValidSurveyId, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Account");
    }

    [Fact]
    public async Task Delete_ServiceReturnsFailure_SetsTempDataErrorMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.DeleteAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Question not found"));

        var controller = BuildController(service, ValidUserId);

        // Act
        await controller.Delete(ValidQuestionId, ValidSurveyId, CancellationToken.None);

        // Assert
        controller.TempData["ErrorMessage"].Should().Be("Question not found");
    }

    [Fact]
    public async Task Delete_ServiceReturnsSuccess_SetsTempDataSuccessMessage()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.DeleteAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);

        // Act
        await controller.Delete(ValidQuestionId, ValidSurveyId, CancellationToken.None);

        // Assert
        controller.TempData["SuccessMessage"].Should().Be("Question deleted");
    }

    [Fact]
    public async Task Delete_ValidRequest_RedirectsToSurveyEdit()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.DeleteAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);

        // Act
        var result = await controller.Delete(ValidQuestionId, ValidSurveyId, CancellationToken.None);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Edit");
        redirect.ControllerName.Should().Be("Surveys");
        redirect.RouteValues!["id"].Should().Be(ValidSurveyId);
    }

    [Fact]
    public async Task Delete_ValidRequest_CallsServiceDeleteWithCorrectId()
    {
        // Arrange
        var service = new Mock<IQuestionService>();
        service.Setup(s => s.DeleteAsync(ValidQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = BuildController(service, ValidUserId);

        // Act
        await controller.Delete(ValidQuestionId, ValidSurveyId, CancellationToken.None);

        // Assert
        service.Verify(s => s.DeleteAsync(ValidQuestionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}