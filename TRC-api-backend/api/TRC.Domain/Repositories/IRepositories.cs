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

public interface IPhoneProfileRepository : IRepository<PhoneProfile>
{
    Task<PhoneProfile?> GetByPhoneAsync(string normalizedPhone, CancellationToken ct = default);
}

public interface IOtpCodeRepository : IRepository<OtpCode>
{
    // Newest unverified, unexpired code for this phone.
    Task<OtpCode?> GetActiveAsync(Guid phoneProfileId, DateTime nowUtc, CancellationToken ct = default);
    Task<int> CountSentSinceAsync(Guid phoneProfileId, DateTime sinceUtc, CancellationToken ct = default);
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
    Task<IReadOnlyList<Appointment>> GetForPhoneAsync(Guid phoneProfileId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetForDayAsync(Guid consultationDayId, CancellationToken ct = default);
}

// Commits changes across repositories in one transaction.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
