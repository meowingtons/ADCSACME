using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace xACME.Models.DbContexts
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static void SetDbProvider(this DbContextOptionsBuilder options, IConfiguration configuration)
        {
            switch (configuration["Provider"])
            {
                // Check Provider and get ConnectionString
                case "SQLite":
                    options.UseSqlite(configuration.GetConnectionString("SQLite"));
                    break;
                case "MySQL":
                    options.UseMySql(configuration.GetConnectionString("MySQL"));
                    break;
                case "MSSQL":
                    options.UseSqlServer(configuration.GetConnectionString("MSSQL"));
                    break;
                case "PostgreSQL":
                    options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
                    break;
                default:
                    throw new ArgumentException("Not a valid database type");
            }
        }
    }
}
