// <copyright file="SurveyCsvExporter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Exporters;

using System.Text;

public static class SurveyCsvExporter
{
    public static byte[] GenerateResponsesCsv(SurveyResponsesExportModel model)
    {
        var sb = new StringBuilder();

        foreach (var response in model.Responses)
        {
            AppendResponse(sb, response);
        }

        return Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();
    }

    private static void AppendResponse(StringBuilder sb, SurveyResponseExportModel response)
    {
        sb.AppendLine($"=== {response.RespondentName} ===");
        sb.AppendLine($"Email: {response.RespondentEmail}");
        sb.AppendLine($"Submitted: {response.SubmittedAt:g}");
        sb.AppendLine();

        sb.AppendLine("Question,Answer");

        foreach (var answer in response.Answers)
        {
            AppendAnswer(sb, answer);
        }

        sb.AppendLine();
        sb.AppendLine("====================================");
        sb.AppendLine();
    }

    private static void AppendAnswer(StringBuilder sb, SurveyResponseAnswerExportModel answer)
    {
        var answerText = GetAnswerText(answer);
        var safeAnswerText = SanitizeCsvValue(answerText);
        var safeQuestionText = SanitizeCsvValue(answer.QuestionText);

        sb.AppendLine($"\"{safeQuestionText}\",\"{safeAnswerText}\"");
    }

    private static string GetAnswerText(SurveyResponseAnswerExportModel answer)
    {
        if (!string.IsNullOrWhiteSpace(answer.TextAnswer))
        {
            return answer.TextAnswer;
        }

        if (answer.SelectedOptionTexts.Any())
        {
            return string.Join(", ", answer.SelectedOptionTexts);
        }

        return "-";
    }

    private static string SanitizeCsvValue(string value)
    {
        return value.Replace("\"", "'").Replace("\n", " ");
    }
}