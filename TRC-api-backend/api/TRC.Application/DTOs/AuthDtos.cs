using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, UserRole Role, Language PreferredLanguage);
public record LoginRequest(string Email, string Password);
public record AuthResult(string AccessToken, string RefreshToken, string FullName, UserRole Role, Language PreferredLanguage);
public record RefreshRequest(string RefreshToken);
