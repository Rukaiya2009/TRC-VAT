using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TRC.Application.Common;
using TRC.Application.Options;
using TRC.API.Middleware;
using TRC.Infrastructure.Auth;
using TRC.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration-bound options (rates & gauge thresholds live in config) ----
builder.Services.Configure<TaxRateOptions>(builder.Configuration.GetSection(TaxRateOptions.SectionName));
builder.Services.Configure<VarianceOptions>(builder.Configuration.GetSection(VarianceOptions.SectionName));

// ---- Application + Infrastructure layers ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- Auth (JWT bearer) ----
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(jwt.Key)
                    ? "dev-only-insecure-key-change-me-32chars!!" : jwt.Key)),
        };
    });
builder.Services.AddAuthorization();

// ---- CORS (allow the Vercel frontend + local dev; preview URLs are wildcarded) ----
const string CorsPolicy = "trc-web";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.SetIsOriginAllowed(_ => true) // TODO: tighten to Vercel domains + previews before production
     .AllowAnyHeader().AllowAnyMethod()));

// ---- Localization scaffold (en / bn) — FR-12.2 ----
builder.Services.AddLocalization();

// Enums serialize as their names ("Admin", "Low", "VAT") instead of integers,
// and incoming role/language strings are accepted and validated.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TRC VAT Risk Checker API", Version = "v1" });

    // Authorize button so protected endpoints are testable from Swagger (NFR-7).
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request localization (Accept-Language drives en/bn).
var supported = new[] { "en", "bn" };
app.UseRequestLocalization(o =>
{
    o.SetDefaultCulture("en").AddSupportedCultures(supported).AddSupportedUICultures(supported);
});

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();