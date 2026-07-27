# TRC API — Identity + Email Pivot: what changed & how to run

This zip replaces your `TRC-api-backend/api` source. OTP is gone; prospects are now
Identity accounts (email + password + phone), email must be confirmed before booking,
and login has 5-fail/15-min lockout. Quick Check stays public.

## 1. Restore + build

```bash
cd TRC-api-backend/api
dotnet restore
dotnet build
```

If the FIRST build shows errors, paste them to me — a 25-file refactor without a compiler
in the loop usually needs a small touch-up or two. Most likely area: a stray reference I
couldn't see, or an Identity package version. Everything is pinned to the 8.0.x line.

## 2. Database — clean path (recommended, since all current data is throwaway)

Your DB currently has the old OTP/PhoneProfile schema. The cleanest reset:

**a.** In Supabase SQL editor, wipe the schema:
```sql
drop schema public cascade;
create schema public;
```

**b.** Delete the two OLD migration files + snapshot so EF regenerates cleanly:
```
TRC.Infrastructure/Migrations/20260706045633_InitialCreate.cs (+ .Designer.cs)
TRC.Infrastructure/Migrations/20260717145905_Phase4BookingOtp.cs (+ .Designer.cs)
TRC.Infrastructure/Migrations/AppDbContextModelSnapshot.cs
```

**c.** Generate one fresh migration (includes Identity tables + the new model) and apply:
```bash
export TRC_MIGRATIONS_CONNECTION="Host=...pooler.supabase.com;Database=postgres;Username=postgres.<ref>;Password=<pw>;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
dotnet ef migrations add InitialCreate -p TRC.Infrastructure -s TRC.API
dotnet ef database update -p TRC.Infrastructure -s TRC.API
```

(Alternative, keeps history: skip a/b and just `migrations add IdentityEmailPivot` on top —
but the diff is large, so the clean path above is safer.)

## 3. Config to set

- `appsettings.json` → `ZeptoMail:Token` — paste your ZeptoMail **Send Mail** token
  (format `Zoho-enczapikey xxxx`). Until then, emails just log and confirmation tokens are
  returned in the API response (see `AuthDev:DevReturnTokens`).
- `AuthDev:DevReturnTokens` is `true` for testing. **Set false in production.**

## 4. New Swagger funnel (no OTP)

1. `POST /api/Auth/register` — `{ email, password, fullName, phone, preferredLanguage }`.
   Response includes `devConfirmToken` (because DevReturnTokens is on).
2. `POST /api/Auth/confirm-email` — `{ email, token: <devConfirmToken> }`.
3. `POST /api/Auth/login` — now allowed. (Try a wrong password 5× first to watch lockout fire.)
4. **Authorize** in Swagger with the returned `accessToken` (no quotes, no "Bearer").
5. Seed/create an Admin (`create-staff` needs an existing Admin — for the very first Admin,
   either register then flip the role in DB once, or temporarily seed one). Publish a day.
6. `POST /api/appointments` as the confirmed prospect → `Booked`.
7. `GET /api/appointments/mine` → `DELETE /{id}` → cancelled.

## Security fixes included
- Public `register` can no longer set a role — it always creates a Prospect. The old
  self-register-as-Admin hole is closed. Staff come from Admin-only `create-staff`.
- Password hashing, lockout, reset & confirmation tokens are all Identity's (hardened),
  not hand-rolled.

## First-Admin note
`create-staff` is Admin-only, so you need one Admin to bootstrap. Quickest for testing:
register normally, then in Supabase set that user's `Role` to `Admin` and `EmailConfirmed`
to true once — thereafter use `create-staff`. I can add a one-time seeded Admin (from config)
if you'd prefer; say the word.
