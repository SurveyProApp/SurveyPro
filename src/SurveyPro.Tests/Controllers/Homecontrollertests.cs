// <copyright file="HomeControllerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Controllers;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SurveyPro.Application.Interfaces;
using SurveyPro.Web.Controllers;
using SurveyPro.Web.ViewModels;
using Xunit;

/// <summary>
/// Unit tests for <see cref="HomeController"/>.
/// </summary>
public class HomeControllerTests
{
    private static HomeController BuildController()
    {
        var quoteService = new Mock<IQuoteService>();
        quoteService
            .Setup(service => service.GetQuoteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SurveyPro.Application.DTOs.ExternalApis.QuoteOfTheDayDto?)null);

        var controller = new HomeController(quoteService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
        return controller;
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = controller.Privacy();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ReturnsViewWithErrorViewModel()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = controller.Error();

        // Assert
        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().BeOfType<ErrorViewModel>();
    }

    [Fact]
    public void Error_ShouldSetRequestId_AndShowRequestIdIsTrue()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = controller.Error();

        // Assert
        var model = result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void Error_ErrorViewModel_ShowRequestId_TrueWhenRequestIdSet()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "test-id" };

        // Act & Assert
        model.ShowRequestId.Should().BeTrue();
    }
}