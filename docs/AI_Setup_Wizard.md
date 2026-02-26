# AI Setup Wizard

System initialization on first run. Available when `installed.lock` file does not exist.

## Access

```
http://<host>:<port>/setup/index.html
```

## API Endpoints

All under `/api/Setup/` with `AllowAnonymous`.

| Method | Action | Body | Description |
|--------|--------|------|-------------|
| GET | `Status` | — | Check installation status |
| POST | `TestDatabase` | `{ Server, Port, Database, User, Password }` | Test DB connection |
| POST | `TestRedis` | `{ Connection, InstanceName }` | Test Redis connection |
| POST | `Install` | `SetupRequest` (see below) | Execute installation |

## SetupRequest Schema

```json
{
  "Database": { "Server": "", "Port": "3306", "Database": "Saibo", "User": "root", "Password": "" },
  "EnableCache": false,
  "CacheProvider": "Memory|Redis",
  "Redis": { "Connection": "localhost:6379", "InstanceName": "SB:" },
  "AppName": "赛博管理系统",
  "AdminUsername": "admin",
  "AdminPassword": "123456",
  "AdminDisplayName": "超级管理员",
  "CorsAllowAll": true,
  "CorsOrigins": [],
  "PasswordStorageMode": "Encrypted|PlainText"
}
```

## Installation Process

1. Create database tables (FreeSql auto-sync)
2. Seed data: admin role, initial menus, admin account
3. Update `appsettings.json` (DB, cache, JWT keys, CORS)
4. Create `installed.lock` marker file
5. **Requires app restart** to load new config

## Re-install

Delete `installed.lock` and restart app.

## Related Files

- `SB.API/Controllers/SetupController.cs`
- `SB.Services/SetupService.cs`
- `SB.API/Middleware/SetupMiddleware.cs`
- Frontend: `SB.API/wwwroot/setup/index.html`
