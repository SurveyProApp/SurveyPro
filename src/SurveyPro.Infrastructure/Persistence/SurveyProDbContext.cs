// <copyright file="SurveyProDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Infrastructure.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class SurveyProDbContext : DbContext
    {
        public SurveyProDbContext(DbContextOptions<SurveyProDbContext> options)
        : base(options)
        {
        }
    }
}
