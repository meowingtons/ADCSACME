using System;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using xACME.Models.Acme;
using xACME.Models.DbModels;

namespace xACME.Models.DbContexts
{
    public sealed class AcmeContext : DbContext
    {
        public AcmeContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AcmeContextAccountConfiguration());
            modelBuilder.ApplyConfiguration(new AcmeContextOrderConfiguration());
            modelBuilder.ApplyConfiguration(new AcmeContextAuthZConfiguration());
        }

        public DbSet<DbAccount> Accounts { get; set; }
        public DbSet<DbAccountKey> AccountKeys { get; set; }
        public DbSet<DbOrder> Orders { get; set; }
        public DbSet<DbAuthZ> Authorizations { get; set; }
        public DbSet<DbChallenge> Challenges { get; set; }
    }

    public class AcmeContextAccountConfiguration : IEntityTypeConfiguration<DbAccount>
    {
        public void Configure(EntityTypeBuilder<DbAccount> builder)
        {
            builder.Property(e => e.InitialIp).HasConversion(v => v.ToString(), v => IPAddress.Parse(v));
            builder.Property(e => e.Contact).HasConversion(v => string.Join(",", v.ToArray()),
                v => v.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).ToList());
        }
    }

    public class AcmeContextOrderConfiguration : IEntityTypeConfiguration<DbOrder>
    {
        public void Configure(EntityTypeBuilder<DbOrder> builder)
        {
            builder.Property(x => x.Identifiers).HasConversion(v => string.Join(",", v.Select(x => x.value)),
                v => AuthorizationIdentifier.AuthZDbParser(v.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()));
        }
    }

    public class AcmeContextAuthZConfiguration : IEntityTypeConfiguration<DbAuthZ>
    {
        public void Configure(EntityTypeBuilder<DbAuthZ> builder)
        {
            builder.Property(x => x.Identifier).HasConversion(x => x.value,
                v => new AuthorizationIdentifier {Type = AuthZIdentifierType.dns, value = v});
        }
    }
}
