// <copyright file="SurveyProDbContextFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Persistence;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SurveyProDbContextFactory : IDesignTimeDbContextFactory<SurveyProDbContext>
{
    public SurveyProDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<SurveyProDbContextFactory>()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<SurveyProDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new SurveyProDbContext(optionsBuilder.Options);
    }
}