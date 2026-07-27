using TRC.Domain.Entities;

namespace TRC.Domain.Repositories;

// Generic repo for BaseEntity-derived aggregates. NOTE: User is NOT a BaseEntity anymore
// (it's an IdentityUser), so it has its own repository below.
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
    Task<Import?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Import>> GetHistoryForConsigneeAsync(string consignee, CancellationToken ct = default);
}

public interface IRiskRuleRepository : IRepository<RiskRule>
{
    Task<IReadOnlyList<RiskRule>> GetEnabledAsync(CancellationToken ct = default);
    Task<RiskRule?> GetByCodeAsync(string code, CancellationToken ct = default);
}

// Standalone (User is an IdentityUser, not a BaseEntity). Auth itself uses UserManager;
// this is for booking-side reads/writes of the User's block state and contact info.
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByPhoneAsync(string normalizedPhone, CancellationToken ct = default);
    void Update(User user);
}

public interface IConsultationDayRepository : IRepository<ConsultationDay>
{
    Task<ConsultationDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<ConsultationDay?> GetWithAppointmentsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ConsultationDay>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<Appointment?> GetWithDayAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetForDayAsync(Guid consultationDayId, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
