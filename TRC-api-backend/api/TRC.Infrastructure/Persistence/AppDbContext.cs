using Microsoft.EntityFrameworkCore;
using TRC.Domain.Entities;
using TRC.Domain.Enums;

namespace TRC.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneProfile> PhoneProfiles => Set<PhoneProfile>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Import> Imports => Set<Import>();
    public DbSet<TaxBreakdown> TaxBreakdowns => Set<TaxBreakdown>();
    public DbSet<QuickCheck> QuickChecks => Set<QuickCheck>();
    public DbSet<BusinessPeriodData> BusinessPeriodData => Set<BusinessPeriodData>();
    public DbSet<Commodity> Commodities => Set<Commodity>();
    public DbSet<RiskRule> RiskRules => Set<RiskRule>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<ConsultationDay> ConsultationDays => Set<ConsultationDay>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // All monetary values decimal(18,2) per the design constraint (SRS §2.4).
        foreach (var prop in b.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            prop.SetColumnType("decimal(18,2)");

        RiskRuleSeed.Seed(b);
    }
}
