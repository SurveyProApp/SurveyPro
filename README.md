# SurveyPro

SurveyPro is an ASP.NET Core web application for creating, publishing, completing, and reviewing surveys.

The solution is built with a clean layered architecture and currently supports full end-to-end survey flow:

- Author can create and manage surveys and questions.
- Respondent can join by access code, save draft answers, clear answers, and submit.
- Author and Admin can view all submitted responses.

## Key Features

### Author

- Create survey (title, description, public/private).
- Edit survey details.
- Add, edit, and delete questions.
- Publish survey.
- View personal surveys list.
- Open respondent link by access code.
- View all submitted responses for own survey.

### Respondent

- Join survey by access code.
- Fill survey with support for question types:
  - Text
  - SingleChoice
  - MultipleChoice
  - Likert
- Save draft answers.
- Clear draft answers.
- Submit survey.

### Admin

- View all submitted responses for surveys.

## Tech Stack

- .NET 9
- ASP.NET Core MVC
- Entity Framework Core + PostgreSQL (Npgsql)
- ASP.NET Core Identity (users and roles)
- Serilog (Console, File, Seq)
- xUnit + Moq + FluentAssertions

## Solution Structure

The repository follows layered separation:

- src/SurveyPro.Domain: domain entities and enums
- src/SurveyPro.Application: use cases, interfaces, DTOs, business services
- src/SurveyPro.Infrastructure: persistence, repositories, EF migrations, identity helpers
- src/SurveyPro.Web: MVC UI, controllers, views, startup configuration
- src/SurveyPro.Tests: unit tests for services and controllers

## Roles

Roles are seeded at startup:

- Respondent
- Author
- Admin

## Getting Started

### 1. Prerequisites

- .NET SDK 9.0+
- PostgreSQL 15+ (or compatible)
- Optional: Seq at http://localhost:5341 for centralized logs

### 2. Clone and Restore

```bash
git clone <your-repository-url>
cd SurveyPro
dotnet restore
```

### 3. Configure Connection String (Recommended via User Secrets)

The web project uses User Secrets in development.

Initialize user secrets if needed:

```bash
dotnet user-secrets init --project src/SurveyPro.Web
```

Set the connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SurveyProDb;Username=postgres;Password=your_password" --project src/SurveyPro.Web
```

You can also set it in src/SurveyPro.Web/appsettings.json or environment variables, but secrets are preferred for local development.

### 4. Run the Application

```bash
dotnet run --project src/SurveyPro.Web
```

On startup, the app will:

- apply pending EF Core migrations
- seed Identity roles

Default route:

- /Home/Index

## Running Tests

Run all tests:

```bash
dotnet test src/SurveyPro.Tests/SurveyPro.Tests.csproj
```

The test project is configured to collect coverage output in:

- src/SurveyPro.Tests/TestResults

## Implemented Response Viewing Use Case

Survey responses page is implemented for Author and Admin.

Main flow:

- Author opens My Surveys and selects View responses.
- Admin can open View responses from available surveys.
- System shows all submitted responses grouped by respondent with question-by-question answers.

Current output includes:

- total submitted response count
- respondent identity information
- submission timestamp
- normalized answer details per question

This structure is intentionally prepared for future use cases:

- response statistics tables
- charts and histograms
- export to PDF and CSV

## Logging

Serilog is configured to write logs to:

- Console
- Rolling file logs under src/SurveyPro.Web/Logs
- Seq (http://localhost:5341)

## Development Notes

- Nullable reference types are enabled.
- Warnings as errors are enabled in the web project.
- StyleCop analyzers are configured.

## Useful Commands

Build solution:

```bash
dotnet build SurveyPro.sln
```

Run web app:

```bash
dotnet run --project src/SurveyPro.Web
```

Run tests:

```bash
dotnet test src/SurveyPro.Tests/SurveyPro.Tests.csproj --no-build
```

## Future Enhancements

- Survey analytics module (aggregations, trends, filters)
- Data visualization dashboards
- Export module (PDF/CSV)
- Pagination and search for large response sets
- Improved audit trail and moderation tools
