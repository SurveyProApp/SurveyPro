// <copyright file="SurveyPdfExporter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Exporters;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class SurveyPdfExporter
{
    public static byte[] GenerateResponsesPdf(SurveyResponsesExportModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container => BuildDocument(container, model)).GeneratePdf();
    }

    private static void BuildDocument(IDocumentContainer container, SurveyResponsesExportModel model)
    {
        container.Page(page =>
        {
            ConfigurePage(page, model);
        });
    }

    private static void ConfigurePage(PageDescriptor page, SurveyResponsesExportModel model)
    {
        page.Margin(30);

        page.Header()
            .Text($"Survey Results: {model.SurveyTitle}")
            .FontSize(20)
            .Bold();

        page.Content()
            .Column(column => RenderContent(column, model));

        page.Footer()
            .AlignCenter()
            .Text(text =>
            {
                text.Span("Generated at ");
                text.Span(DateTime.UtcNow.ToString("g"));
            });
    }

    private static void RenderContent(ColumnDescriptor column, SurveyResponsesExportModel model)
    {
        column.Spacing(15);

        column.Item().Text($"Description: {model.SurveyDescription}");
        column.Item().Text($"Access Code: {model.AccessCode}");
        column.Item().Text($"Total Responses: {model.TotalSubmittedResponses}");

        foreach (var response in model.Responses)
        {
            RenderResponse(column, response);
        }
    }

    private static void RenderResponse(ColumnDescriptor column, SurveyResponseExportModel response)
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
                RenderAnswer(inner, answer);
            }
        });
    }

    private static void RenderAnswer(ColumnDescriptor inner, SurveyResponseAnswerExportModel answer)
    {
        inner.Item().PaddingTop(5).Column(answerColumn =>
        {
            answerColumn.Item()
                .Text($"{answer.QuestionOrderNumber}. {answer.QuestionText}")
                .SemiBold();

            if (!string.IsNullOrWhiteSpace(answer.TextAnswer))
            {
                answerColumn.Item().Text($"Answer: {answer.TextAnswer}");
                return;
            }

            if (answer.SelectedOptionTexts.Any())
            {
                answerColumn.Item().Text($"Selected: {string.Join(", ", answer.SelectedOptionTexts)}");
                return;
            }

            answerColumn.Item().Text("Answer: -");
        });
    }
}