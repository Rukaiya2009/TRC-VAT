namespace TRC.Application.DTOs;

public record SendOtpRequest(string Phone);

// DevCode is populated only while Otp:DevReturnCode is true (pre-D4 testing). Null in production.
public record SendOtpResult(string Phone, int ExpiresInSeconds, string? DevCode);

public record VerifyOtpRequest(string Phone, string Code);

// PhoneToken is a short-lived, phone-scoped JWT. Every booking endpoint requires it.
public record VerifyOtpResult(string Phone, bool Verified, string PhoneToken, int ExpiresInSeconds);
