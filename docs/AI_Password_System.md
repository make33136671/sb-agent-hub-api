# AI Password System

## Storage Modes

| Mode | Storage | Use Case |
|------|---------|----------|
| **Encrypted** | BCrypt hash | Production (recommended) |
| **PlainText** | Raw string | Dev/test only |

Configured during setup wizard, stored in `appsettings.json` → `PasswordConfig.StorageMode`. Permanent after installation.

## PasswordUtil (`SB.Core/Util/PasswordUtil.cs`)

| Method | Behavior |
|--------|----------|
| `IsEncryptedMode()` | Returns true if encrypted mode |
| `EncryptPassword(password)` | BCrypt hash or passthrough |
| `VerifyPassword(input, stored)` | BCrypt verify with plaintext fallback |

## Usage

```csharp
// Verify login
if (!PasswordUtil.VerifyPassword(inputPassword, admin.Password))
    return ResultData.No("Wrong password!");

// Create/update password
admin.Password = PasswordUtil.EncryptPassword(newPassword);
```

## Plaintext → Encrypted Migration

Seamless, no manual intervention:
1. Enable encrypted mode in config
2. Old admins: plaintext passwords still work (auto-downgrade verification)
3. New passwords: BCrypt encrypted
4. Full migration completes as users change passwords

## Related Files

- `SB.Core/Util/PasswordUtil.cs` — Core utility
- `SB.Services/AdminService.cs` — Login/password change
- `SB.Services/SetupService.cs` — Initial admin creation
- Dependency: `BCrypt.Net-Next` v4.0.3
