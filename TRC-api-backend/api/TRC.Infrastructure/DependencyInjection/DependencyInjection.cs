using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TRC.Application.Interfaces;
using TRC.Domain.Entities;
using TRC.Domain.Repositories;
using TRC.Infrastructure.Auth;
using TRC.Infrastructure.Notifications;
using TRC.Infrastructure.Persistence;
using TRC.Infrastructure.Repositories;
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

        // ---- ASP.NET Core Identity (email accounts, lockout, confirmation, reset) ----
        services.AddIdentityCore<User>(o =>
        {
            o.Password.RequiredLength = 8;
            o.Password.RequireDigit = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequireLowercase = false;
            o.User.RequireUniqueEmail = true;
            o.SignIn.RequireConfirmedEmail = true;          // login blocked until email confirmed
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            o.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Repositories & unit of work
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IImportRepository, ImportRepository>();
        services.AddScoped<IRiskRuleRepository, RiskRuleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConsultationDayRepository, ConsultationDayRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cross-cutting infrastructure services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Email delivery via ZeptoMail HTTP API (Render blocks SMTP). Falls back to logging
        // if no token is configured, so dev runs without a key.
        services.AddScoped<INotificationService, ZeptoMailNotificationService>();

        // Swap for GoogleCalendarMeetingLinkProvider once TRC supplies Workspace credentials.
        services.AddScoped<IMeetingLinkProvider, ManualMeetingLinkProvider>();

        return services;
    }
}
