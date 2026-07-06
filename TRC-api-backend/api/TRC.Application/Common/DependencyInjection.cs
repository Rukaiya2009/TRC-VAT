using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TRC.Application.Interfaces;
using TRC.Application.Services;

namespace TRC.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        services.AddScoped<IVarianceService, VarianceService>();
        services.AddScoped<IRiskEngine, RiskEngine>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
