using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyPro.Infrastructure.Persistence
{
    public class SurveyProDbContext : DbContext
    {
        public SurveyProDbContext(DbContextOptions<SurveyProDbContext> options)
        : base(options)
        {
        }
    }
}
