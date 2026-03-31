// <copyright file="QuestionsController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyPro.Application.DTOs.Questions;
using SurveyPro.Application.Interfaces;
using SurveyPro.Web.ViewModels.Questions;
using System.Security.Claims;

[Authorize(Roles = "Author")]
public class QuestionsController : BaseController
{
    private readonly IQuestionService questionService;

    public QuestionsController(IQuestionService questionService)
    {
        this.questionService = questionService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Edit", "Surveys", new { id = model.SurveyId });
        }

        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return RedirectToAction("Login", "Account");
        }

        var dto = new CreateQuestionRequestDto
        {
            SurveyId = model.SurveyId,
            Text = model.Text,
            Type = model.Type,
            Options = model.Options?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList() ?? new List<string>(),
        };

        var result = await questionService.CreateAsync(userIdResult.Value, dto, cancellationToken);

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Question added";
        }

        return RedirectToAction("Edit", "Surveys", new { id = model.SurveyId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await questionService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var q = result.Value!;

        return View(new EditQuestionViewModel
        {
            Id = q.Id,
            SurveyId = q.SurveyId,
            Text = q.Text,
            Type = q.Type,
            Options = q.Options ?? new (),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditQuestionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return RedirectToAction("Login", "Account");
        }

        var dto = new UpdateQuestionRequestDto
        {
            Text = model.Text,
            Type = model.Type,
            Options = model.Options?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList() ?? new List<string>(),
        };

        var result = await questionService.UpdateAsync(model.Id, userIdResult.Value, dto, cancellationToken);

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Question updated";
        }

        return RedirectToAction("Edit", "Surveys", new { id = model.SurveyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid surveyId, CancellationToken cancellationToken)
    {
        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await questionService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Question deleted";
        }

        return RedirectToAction("Edit", "Surveys", new { id = surveyId });
    }
}
