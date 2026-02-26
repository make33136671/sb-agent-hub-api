# AI Build Guide

## Environment

- .NET 8.0 SDK, MySQL 5.7+, Redis 6.0+ (optional)

## Commands

```bash
dotnet build                           # Debug build
dotnet build -c Release                # Release build
dotnet run --project SB.API            # Dev: HTTP 5153, HTTPS 7292
dotnet watch run --project SB.API      # Hot reload
dotnet publish -c Release -o ./dist    # Publish
dotnet SB.API/bin/Release/net8.0/SB.API.dll --urls='http://127.0.0.1:6000'  # Prod
```

## New Feature Workflow

### 1. Entity

```csharp
[Table(Name = "SB_YourEntity")]
public class YourEntity : IEntity, ISoftDelete
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    [Column(DbType = "timestamp", InsertValueSql = "now()")]
    public DateTime CreatedTime { get; set; }
}
```

### 2. Service

```csharp
[AutoDependencyInjection]
public class YourEntityService : BaseCrudService<YourEntity>
{
    public YourEntityService(IFreeSql fsql, ILogger<YourEntityService> logger)
        : base(fsql, logger) { }
}
```

### 3. Controller

```csharp
// Admin (auto-prefixed to /admin/YourEntity/[action])
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class YourEntityController : Controller
{
    private readonly YourEntityService _service;
}

// Public API (absolute route, bypasses convention)
[ApiController, Authorize(AuthenticationSchemes = "JwtApi"), Route("/api/[controller]/[action]")]
public class YourApiController : Controller { }
```

## Key Conventions

- English names only (Admin not Guanliyuan)
- `[AutoDependencyInjection]` for singleton registration
- Soft delete: `IsDeleted` flag, global FreeSql filter auto-excludes
- `[FromForm]` for form data, `[FromBody]` for JSON

## Config: appsettings.json

```json
{
  "CacheConfig": { "Enabled": false, "Provider": "Memory", "Redis": { "Connection": "localhost:6379", "InstanceName": "SB:" } },
  "Logging": { "LogLevel": { "Default": "Debug", "Microsoft": "Information" } }
}
```

## Common Build Errors

| Error | Fix |
|-------|-----|
| `CS0234: Namespace not found` | Check `.csproj` `<ProjectReference>` |
| `Invalid default value for 'CreatedTime'` | Add `[Column(DbType = "timestamp", InsertValueSql = "now()")]` |
