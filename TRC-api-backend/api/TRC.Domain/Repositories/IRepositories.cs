using TRC.Domain.Entities;

namespace TRC.Domain.Repositories;

// Generic repository abstraction (implemented in Infrastructure).
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IImportRepository : IRepository<Import>
{
    // Includes TaxBreakdowns + latest RiskAssessment for report/assess flows.
    Task<Import?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Import>> GetHistoryForConsigneeAsync(string consignee, CancellationToken ct = default);
}

public interface IRiskRuleRepository : IRepository<RiskRule>
{
    Task<IReadOnlyList<RiskRule>> GetEnabledAsync(CancellationToken ct = default);
    Task<RiskRule?> GetByCodeAsync(string code, CancellationToken ct = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

// Commits changes across repositories in one transaction.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
