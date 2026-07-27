using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

// Public registration only ever creates a Prospect — no Role field, closing the
// self-register-as-Admin hole. Staff are created via the Admin-only path.
public record RegisterRequest(string Email, string Password, string FullName, string Phone, Language PreferredLanguage);
public record LoginRequest(string Email, string Password);
public record AuthResult(string AccessToken, string RefreshToken, string FullName, UserRole Role, Language PreferredLanguage, bool EmailConfirmed);
public record RefreshRequest(string RefreshToken);

// Email confirmation + password reset (Identity token flows). DevToken is populated only
// while Otp:DevReturnCode-style dev mode is on, so the flow is testable without a live inbox.
public record RegisterResult(AuthResult Auth, string? DevConfirmToken);
public record ConfirmEmailRequest(string Email, string Token);
public record ResendConfirmationRequest(string Email);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

// Admin-only staff creation.
public record CreateStaffRequest(string Email, string Password, string FullName, UserRole Role, Language PreferredLanguage);
