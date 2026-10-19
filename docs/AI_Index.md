# AI Documentation Index

## Documents

| File | Content |
|------|---------|
| [AI_API_Reference](AI_API_Reference.md) | All endpoints, params, auth |
| [AI_Build_Guide](AI_Build_Guide.md) | Build, run, dev workflow, new entity/service/controller |
| [AI_Password_System](AI_Password_System.md) | BCrypt, plaintext, migration |
| [AI_Cache_System](AI_Cache_System.md) | CacheNullService, Memory, Redis |
| [AI_Setup_Wizard](AI_Setup_Wizard.md) | Installation API, config structure |

## Quick Reference

- Main instruction file: `CLAUDE.md` (project root)
- Route convention: `AdminRoutePrefixConvention` auto-adds `admin/` prefix
- Auth schemes: `JwtAdmin` (admin 24h), `JwtApi` (API 7d)
- ORM: FreeSql + MySQL, tables prefixed `SB_`
- Response: All APIs return `ResultData` wrapper
