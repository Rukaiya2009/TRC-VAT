using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TRC.Application.Interfaces;
using TRC.Domain.Repositories;
using TRC.Infrastructure.Auth;
using TRC.Infrastructure.Notifications;
using TRC.Infrastructure.Persistence;
using TRC.Infrastructure.Repositories;
using TRC.Infrastructure.Security;
using TRC.Infrastructure.Meetings;

namespace TRC.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        // Repositories & unit of work
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IImportRepository, ImportRepository>();
        services.AddScoped<IRiskRuleRepository, RiskRuleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPhoneProfileRepository, PhoneProfileRepository>();
        services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
        services.AddScoped<IConsultationDayRepository, ConsultationDayRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cross-cutting infrastructure services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<INotificationService, LoggingNotificationService>();
        services.AddScoped<IOtpHasher, OtpHasher>();
        services.AddScoped<IPhoneTokenGenerator, PhoneTokenGenerator>();

        // Swap for GoogleCalendarMeetingLinkProvider once TRC supplies Workspace credentials (D4/Meet).
        services.AddScoped<IMeetingLinkProvider, ManualMeetingLinkProvider>();

        return services;
    }
}
