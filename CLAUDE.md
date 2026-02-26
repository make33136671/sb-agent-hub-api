# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SB (赛博管理系统)** - A .NET 8.0 ASP.NET Core management system with MySQL database and layered architecture.

## Build & Run Commands

```bash
# Build
dotnet build                    # Build all projects
dotnet build -c Release         # Release build

# Run (development)
dotnet run --project SB.API     # HTTP: 5153, HTTPS: 7292

# Run (production)
dotnet SB.API.dll --urls='http://127.0.0.1:6000'
```

## Architecture

### Project Structure
- **SB.Models** - Entity models and DTOs (Admin, User, Menu, AdminRole, AdminRoleMenu, UserRecord, etc.)
- **SB.Core** - Infrastructure: JWT auth, extensions, utilities, FreeSql helpers
- **SB.Services** - Business logic layer with repository pattern
- **SB.API** - API controllers and configuration

### Key Patterns

**Service Registration:** Services use `[AutoDependencyInjection]` attribute for automatic singleton registration. Services extend `BaseRepository<T>` for CRUD operations.

**Dual JWT Authentication:**
- `JwtAdmin` - Admin authentication (24h expiry)
- `JwtApi` - API user authentication (7-day expiry)

**Caching:** `ICacheService` abstraction with Redis (`CacheRedisService`) and Memory (`CacheMemoryCacheService`) implementations.

**Soft Delete:** Entities use `IsDeleted` boolean flag; global FreeSql filter applied.

**Standard Response:** All APIs return `ResultData` wrapper with code, message, type, and data fields.

### Database
- **ORM:** FreeSql with MySQL provider
- **Tables:** Prefixed with `SB_` (e.g., `SB_Admin`, `SB_User`, `SB_AdminRole`, `SB_AdminRoleMenu`)
- **Config:** `appsettings.json` DbConfig section (Server, Port, Database, User, Password)
- **Note:** Some `[Column(Name = "...")]` annotations still use legacy pinyin names to match existing DB columns

## Naming Conventions

All code uses English naming. Legacy pinyin names have been replaced:
- Admin (was Guanliyuan) — admin/manager entities
- User (was Yonghu) — user entities
- AdminRole (was GuanliyuanJuese) — admin role entities
- AdminRoleMenu (was GuanliyuanJueseMenu) — role-menu mapping
- IsDeleted (was Weishanchu) — soft delete flag
- Username, Password, Name, Avatar, Status, CreatedTime, etc. — all property names are English

## Controller & Route Conventions

### Route Convention

`AdminRoutePrefixConvention` (in `Extensions/`) auto-prefixes `admin/` to all controllers using relative routes. Absolute routes (starting with `/`) are not affected.

**When creating new controllers, follow these rules:**

| 类型 | Route 写法 | 最终路由 | Auth |
|------|-----------|---------|------|
| Admin 后台 | `[Route("[controller]/[action]")]` | `/admin/[controller]/[action]` | `JwtAdmin` |
| 公开 API | `[Route("/api/[action]")]` | `/api/[action]` | `JwtApi` |
| 无需认证 | `[Route("/api/[controller]/[action]")]` | `/api/[controller]/[action]` | `AllowAnonymous` |

**Key rules:**
- Admin controllers: use **relative route** `[controller]/[action]`, convention auto-adds `admin/` prefix
- API controllers: use **absolute route** starting with `/api/`, bypasses convention
- Controller names: **no `Admin` prefix** — use business name directly (e.g., `MemberController`, not `AdminMemberController`)
- Route casing: `admin` prefix is lowercase; controller/action tokens keep PascalCase

**New Admin controller template:**
```csharp
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class XxxController : Controller { }
```

**New API controller template:**
```csharp
[ApiController, Authorize(AuthenticationSchemes = "JwtApi"), Route("/api/[controller]/[action]")]
public class XxxController : Controller { }
```

### Existing API Routes
- `/admin/Manager/[action]` — Admin CRUD (ManagerController)
- `/admin/Auth/[action]` — Authentication (AuthController)
- `/admin/Menu/[action]` — Menu management (MenuController)
- `/admin/Member/[action]` — Member management (MemberController)
- `/admin/SystemSetting/[action]` — System settings (SystemSettingController)
- `/admin/SystemSettingCategory/[action]` — Setting categories (SystemSettingCategoryController)
- `/admin/EmailConfig/[action]` — Email configuration (EmailConfigController)
- `/admin/LoginLog/[action]` — Login logs (LoginLogController)
- `/admin/BackgroundTask/[action]` — Background tasks (BackgroundTaskController)
- `/api/Setup/[action]` — System setup wizard (SetupController)
- `/api/[action]` — Public API (ApiController)
