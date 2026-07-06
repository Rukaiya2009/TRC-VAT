using Microsoft.EntityFrameworkCore;
using TRC.Domain.Entities;
using TRC.Domain.Repositories;
using TRC.Infrastructure.Persistence;

namespace TRC.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;
    public EfRepository(AppDbContext db) { Db = db; Set = db.Set<T>(); }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync(new object?[] { id }, ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) => await Set.AddAsync(entity, ct);
    public void Update(T entity) => Set.Update(entity);
    public void Remove(T entity) => Set.Remove(entity);
}

public class ImportRepository : EfRepository<Import>, IImportRepository
{
    public ImportRepository(AppDbContext db) : base(db) { }

    public async Task<Import?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await Set.Include(i => i.TaxBreakdowns)
                 .Include(i => i.RiskAssessments)
                 .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Import>> GetHistoryForConsigneeAsync(string consignee, CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(i => i.Consignee == consignee).ToListAsync(ct);

    public override async Task<IReadOnlyList<Import>> ListAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().Include(i => i.TaxBreakdowns).OrderByDescending(i => i.Date).ToListAsync(ct);
}

public class RiskRuleRepository : EfRepository<RiskRule>, IRiskRuleRepository
{
    public RiskRuleRepository(AppDbContext db) : base(db) { }
    public async Task<IReadOnlyList<RiskRule>> GetEnabledAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(r => r.Enabled).ToListAsync(ct);
    public async Task<RiskRule?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(r => r.Code == code, ct);
}

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(u => u.Email == email, ct);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public UnitOfWork(AppDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
