using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TRC.Application.Common;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Application.Options;
using TRC.Domain.Entities;
using TRC.Domain.Enums;
using TRC.Domain.Repositories;

namespace TRC.Application.Services;

// M9 — OTP verification (FR-9.1 / FR-9.2).
// Delivery goes through INotificationService, which is a logging stub until TRC supplies a
// WhatsApp/email provider (dependency D4). Until then Otp:DevReturnCode surfaces the code in
// the response so the full funnel is testable end-to-end.
public class OtpService : IOtpService
{
    private readonly IPhoneProfileRepository _phones;
    private readonly IOtpCodeRepository _codes;
    private readonly IOtpHasher _hasher;
    private readonly IPhoneTokenGenerator _tokens;
    private readonly INotificationService _notify;
    private readonly IUnitOfWork _uow;
    private readonly IDhakaClock _clock;
    private readonly OtpOptions _opts;
    private readonly ILogger<OtpService> _log;

    public OtpService(
        IPhoneProfileRepository phones,
        IOtpCodeRepository codes,
        IOtpHasher hasher,
        IPhoneTokenGenerator tokens,
        INotificationService notify,
        IUnitOfWork uow,
        IDhakaClock clock,
        IOptions<OtpOptions> opts,
        ILogger<OtpService> log)
    {
        _phones = phones;
        _codes = codes;
        _hasher = hasher;
        _tokens = tokens;
        _notify = notify;
        _uow = uow;
        _clock = clock;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<SendOtpResult> SendAsync(SendOtpRequest request, CancellationToken ct = default)
    {
        var phone = PhoneNumber.Normalize(request.Phone);
        if (!PhoneNumber.IsPlausible(phone))
            throw new InvalidOperationException("That phone number doesn't look valid.");

        var profile = await _phones.GetByPhoneAsync(phone, ct);
        if (profile is null)
        {
            profile = new PhoneProfile { PhoneNumber = phone };
            await _phones.AddAsync(profile, ct);
            await _uow.SaveChangesAsync(ct);
        }

        // A blocked number can't even start the funnel (FR-11.6).
        if (profile.IsBlocked)
            throw new InvalidOperationException(
                "This number has been blocked after repeated missed appointments. Please contact TRC directly.");

        // Rate limit: N sends per rolling hour.
        var sentLastHour = await _codes.CountSentSinceAsync(
            profile.Id, _clock.UtcNow.AddHours(-1), ct);
        if (sentLastHour >= _opts.MaxSendsPerHour)
            throw new InvalidOperationException(
                $"Too many codes requested. Please try again in an hour.");

        var code = GenerateCode(_opts.CodeLength);

        var otp = new OtpCode
        {
            PhoneProfileId = profile.Id,
            CodeHash = _hasher.Hash(code),
            ExpiresAt = _clock.UtcNow.AddMinutes(_opts.ExpiryMinutes),
        };
        await _codes.AddAsync(otp, ct);
        await _uow.SaveChangesAsync(ct);

        // Stub today; real WhatsApp/email send lands behind this same call (D4).
        await _notify.SendAsync(phone, Channel.WhatsApp, "OtpCode", profile.PreferredLanguage, ct);

        if (_opts.DevReturnCode)
            _log.LogWarning("[DEV] OTP for {Phone} is {Code} — DevReturnCode must be false in production.", phone, code);

        return new SendOtpResult(
            phone,
            _opts.ExpiryMinutes * 60,
            _opts.DevReturnCode ? code : null);
    }

    public async Task<VerifyOtpResult> VerifyAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var phone = PhoneNumber.Normalize(request.Phone);

        var profile = await _phones.GetByPhoneAsync(phone, ct)
            ?? throw new InvalidOperationException("No code has been requested for this number.");

        if (profile.IsBlocked)
            throw new InvalidOperationException(
                "This number has been blocked after repeated missed appointments. Please contact TRC directly.");

        var otp = await _codes.GetActiveAsync(profile.Id, _clock.UtcNow, ct)
            ?? throw new InvalidOperationException("That code has expired. Please request a new one.");

        if (otp.Attempts >= _opts.MaxAttemptsPerCode)
            throw new InvalidOperationException("Too many incorrect attempts. Please request a new code.");

        otp.Attempts++;

        if (!_hasher.Verify(otp.CodeHash, request.Code.Trim()))
        {
            _codes.Update(otp);
            await _uow.SaveChangesAsync(ct);
            var left = Math.Max(0, _opts.MaxAttemptsPerCode - otp.Attempts);
            throw new InvalidOperationException($"Incorrect code. {left} attempt(s) remaining.");
        }

        otp.VerifiedAt = _clock.UtcNow;
        profile.LastVerifiedAt = _clock.UtcNow;
        _codes.Update(otp);
        _phones.Update(profile);
        await _uow.SaveChangesAsync(ct);

        var token = _tokens.Generate(profile.Id, phone, _opts.PhoneTokenMinutes);

        return new VerifyOtpResult(phone, true, token, _opts.PhoneTokenMinutes * 60);
    }

    // Cryptographically strong, zero-padded so leading zeros survive.
    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString().PadLeft(length, '0');
    }
}
