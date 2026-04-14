// <copyright file="SurveyPdfExporter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Reflection.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SurveyPro.Web.ViewModels.Surveys;

namespace SurveyPro.Web.Services;

public static class SurveyPdfExporter
{
    public static byte[] GenerateResponsesPdf(SurveyResponsesViewModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text($"Survey Results: {model.SurveyTitle}")
                    .FontSize(20)
                    .Bold();

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(15);

                        column.Item().Text($"Description: {model.SurveyDescription}");
                        column.Item().Text($"Access Code: {model.AccessCode}");
                        column.Item().Text($"Total Responses: {model.TotalSubmittedResponses}");

                        foreach (var response in model.Responses)
                        {
                            column.Item().PaddingTop(10).Border(1).Padding(10).Column(inner =>
                            {
                                inner.Spacing(5);

                                inner.Item().Text($"Respondent: {response.RespondentName}")
                                    .Bold();

                                inner.Item().Text($"Email: {response.RespondentEmail}");
                                inner.Item().Text($"Submitted: {response.SubmittedAt:g}");

                                foreach (var answer in response.Answers)
                                {
                                    inner.Item().PaddingTop(5).Column(answerColumn =>
                                    {
                                        answerColumn.Item()
                                            .Text($"{answer.QuestionOrderNumber}. {answer.QuestionText}")
                                            .SemiBold();

                                        if (!string.IsNullOrWhiteSpace(answer.TextAnswer))
                                        {
                                            answerColumn.Item().Text($"Answer: {answer.TextAnswer}");
                                        }
                                        else if (answer.SelectedOptionTexts.Any())
                                        {
                                            answerColumn.Item().Text(
                                                $"Selected: {string.Join(", ", answer.SelectedOptionTexts)}");
                                        }
                                        else
                                        {
                                            answerColumn.Item().Text("Answer: -");
                                        }
                                    });
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated at ");
                        text.Span(DateTime.UtcNow.ToString("g"));
                    });
            });
        }).GeneratePdf();
    }
}