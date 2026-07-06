using Microsoft.EntityFrameworkCore;
using TRC.Domain.Entities;
using TRC.Domain.Enums;

namespace TRC.Infrastructure.Persistence;

// The 12 client-approved rules (TRC-RSK-002 Rev 1.1). Seeded on InitialCreate;
// editable by Admin at runtime thereafter (FR-6.2).
public static class RiskRuleSeed
{
    // Deterministic GUIDs so the seed is idempotent across migrations.
    private static Guid G(int n) => new($"00000000-0000-0000-0000-0000000000{n:D2}");

    private static readonly DateTime Seeded = new(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder b)
    {
        var rules = new[]
        {
            New(1,  "R1",  "Declared VAT > 10% below computed VAT", 15, RulePhase.A),
            New(2,  "R2",  "Return vs financial statements differ > 15%", 20, RulePhase.B),
            New(3,  "R3",  "Supplier on watchlist", 25, RulePhase.B),
            New(4,  "R4",  "Unit value < 90% of commodity benchmark", 20, RulePhase.B),
            New(5,  "R5",  "AV deviates > 50% from 12-month rolling average", 15, RulePhase.A),
            New(6,  "R6",  "Sales below purchases by > 20%", 10, RulePhase.B),
            New(7,  "R7",  "HS code inconsistent with description/history", 15, RulePhase.A),
            New(8,  "R8",  ">=3 of last 6 declarations below benchmark", 20, RulePhase.B),
            New(9,  "R9",  "Large turnover, no audit in 3 years", 10, RulePhase.B),
            New(10, "R10", "Round-number / repeated invoice patterns", 8, RulePhase.A),
            New(11, "R11", "Required document missing/inconsistent", 12, RulePhase.A),
            New(12, "R12", "External intelligence hit", 30, RulePhase.B),
        };
        b.Entity<RiskRule>().HasData(rules);
    }

    private static RiskRule New(int n, string code, string name, int weight, RulePhase phase) => new()
    {
        Id = G(n),
        Code = code,
        Name = name,
        Weight = weight,
        Enabled = true,
        PhaseTag = phase,
        LastReviewed = Seeded,
        CreatedAt = Seeded,
        Rationale = "Approved default (TRC-RSK-002 Rev 1.1).",
    };
}
