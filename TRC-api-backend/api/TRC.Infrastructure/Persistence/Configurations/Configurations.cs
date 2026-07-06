using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRC.Domain.Entities;

namespace TRC.Infrastructure.Persistence.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.HasIndex(u => u.Email).IsUnique();
        e.Property(u => u.Email).HasMaxLength(256).IsRequired();
        e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
    }
}

public class PhoneProfileConfig : IEntityTypeConfiguration<PhoneProfile>
{
    public void Configure(EntityTypeBuilder<PhoneProfile> e)
    {
        e.HasIndex(p => p.PhoneNumber).IsUnique();
        e.Property(p => p.PhoneNumber).HasMaxLength(32).IsRequired();
    }
}

public class ImportConfig : IEntityTypeConfiguration<Import>
{
    public void Configure(EntityTypeBuilder<Import> e)
    {
        e.HasIndex(i => i.HSCode);
        e.HasIndex(i => i.Date);
        e.Property(i => i.HSCode).HasMaxLength(20).IsRequired();
        e.HasMany(i => i.TaxBreakdowns).WithOne(t => t.Import)
            .HasForeignKey(t => t.ImportId).OnDelete(DeleteBehavior.Cascade);
        e.HasMany(i => i.RiskAssessments).WithOne(r => r.Import)
            .HasForeignKey(r => r.ImportId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CommodityConfig : IEntityTypeConfiguration<Commodity>
{
    public void Configure(EntityTypeBuilder<Commodity> e) => e.HasIndex(c => c.HSCode).IsUnique();
}

public class RiskRuleConfig : IEntityTypeConfiguration<RiskRule>
{
    public void Configure(EntityTypeBuilder<RiskRule> e)
    {
        e.HasIndex(r => r.Code).IsUnique();
        e.Property(r => r.Code).HasMaxLength(8).IsRequired();
    }
}

public class BusinessPeriodDataConfig : IEntityTypeConfiguration<BusinessPeriodData>
{
    public void Configure(EntityTypeBuilder<BusinessPeriodData> e) =>
        e.HasIndex(x => new { x.UserId, x.PhoneProfileId, x.Year, x.Month }).IsUnique();
}

public class ConsultationDayConfig : IEntityTypeConfiguration<ConsultationDay>
{
    public void Configure(EntityTypeBuilder<ConsultationDay> e) => e.HasIndex(c => c.Date).IsUnique();
}

// jsonb columns for the assessment rule lists (SRS §7).
public class RiskAssessmentConfig : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> e)
    {
        e.Property(r => r.TriggeredRules).HasColumnType("jsonb");
        e.Property(r => r.NotEvaluable).HasColumnType("jsonb");
    }
}

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> e) => e.Property(a => a.Details).HasColumnType("jsonb");
}
