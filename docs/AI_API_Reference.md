# AI API Reference

All admin endpoints require `JwtAdmin` auth (Bearer token) unless noted. Response: `ResultData` with `code`, `message`, `type`, `data`. POST params: mostly `form`; Setup uses JSON body.

## AuthController `/admin/Auth/[action]`

| Method | Action | Params | Note |
|--------|--------|--------|------|
| POST | Login | `username`, `password`, `totpCode`(optional) form | AllowAnonymous, rate-limited |
| GET | GetInfo | — | Current admin info |
| POST | Logout | — | AllowAnonymous |
| POST | ChangePassword | `oldPassword`, `newPassword` form | |
| POST | ChangeAvatar | `avatar` form | |
| POST | Upload | `file` IFormFile | AllowAnonymous |
| GET | GetGoogleCodeInfo | — | `{ isBound, bindTime }` |
| GET | GetGoogleCodeQR | — | `{ qrBase64, manualEntryKey }` |
| POST | PostBindGoogleCode | `code`, `password` form | |
| POST | UnbindGoogleCode | `totpCode`, `password` form | |

## ManagerController `/admin/Manager/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | query: `username`, `name`, `roleId`, `field`, `order`, `page`, `pageSize` |
| GET | Detail | query: `id` |
| POST | Add | form: `Admin` |
| POST | Edit | form: `Admin` |
| POST | Del | form: `id` |
| GET | RoleList | query: `name`, `roleKey`, `field`, `order`, `page`, `pageSize` |
| GET | RoleDetail | query: `id` |
| POST | RoleAdd | form: `AdminRole` |
| POST | RoleEdit | form: `AdminRole` |
| POST | RoleDel | form: `id` |
| POST | ResetGoogleCode | form: `adminId` |
| GET | GetMenuIds | query: `roleId` |
| GET | GetMenusDetail | query: `roleId` |
| POST | SetMenus | form: `roleId`, `menuIds` (comma-separated) |
| POST | AddMenu | form: `roleId`, `menuId` |
| POST | RemoveMenu | form: `roleId`, `menuId` |
| POST | CopyMenus | form: `fromRoleId`, `toRoleId` |
| GET | GetRolesByMenu | query: `menuId` |
| GET | QueryAdminRole | query: `key` |

## MenuController `/admin/Menu/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | All | — (current admin's permission tree) |
| GET | TreeList | query: `name`, `title`, `field`, `order`, `page`, `pageSize` |
| GET | List | query: same as TreeList |
| GET | Detail | query: `id` |
| GET | QueryMenu | query: `key` |
| POST | Add | form/body: `Menu` |
| POST | Edit | form/body: `Menu` |
| POST | UpdateMenu | form: `id`, `field`, `value` |
| POST | Del | form: `id` |

## MemberController `/admin/Member/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | query: `field`, `order`, `page`, `pageSize` |
| GET | Detail | query: `id` |
| POST | Add | form: `Member` |
| POST | Edit | form: `Member` |
| POST | Del | form: `id` |
| POST | ResetPassword | form: `id` |
| GET | MemberTree | query params |
| GET | Children | query params |
| GET | Search | query params |

## SystemSettingController `/admin/SystemSetting/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | query: `key`, `field`, `order`, `page`, `pageSize` |
| GET | GetCategoryWithSettingList | query: `categoryId`, `field`, `order`, `page`, `pageSize` |
| GET | QuerySystemSetting | query: `key` |
| GET | Detail | query: `id` |
| POST | Add | form/body: `SystemSetting` |
| POST | Edit | form/body: `SystemSetting` |
| POST | SaveList | form: `settings` (JSON string → `List<SystemSetting>`) |
| DELETE | Del | query/form: `id` |

## SystemSettingCategoryController `/admin/SystemSettingCategory/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | query: `key`, `name`, `field`, `order`, `page`, `pageSize` |
| GET | QuerySystemSettingCategory | query: `key` |
| GET | Detail | query: `id` |
| POST | Add | form/body: `SystemSettingCategory` |
| POST | Edit | form/body: `SystemSettingCategory` |
| POST | Del | form: `id` |

## EmailConfigController `/admin/EmailConfig/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | GetConfig | — |
| POST | SaveConfig | form/body: config |
| GET | GetTemplates | — |
| GET | GetTemplate | query: `id` |
| POST | SaveTemplate | form/body: template |
| GET | GetLogs | query params |
| GET | GetLog | query: `id` |

## LoginLogController `/admin/LoginLog/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | query: filter + pagination |

## BackgroundTaskController `/admin/BackgroundTask/[action]`

| Method | Action | Params |
|--------|--------|--------|
| GET | List | — |
| POST | Start | form: task id |
| POST | Stop | form: task id |
| POST | Restart | form: task id |
| POST | ReloadParameters | form: task id |
| GET | GetLogs | query: task id |
| GET | LogList | query: pagination |

## SetupController `/api/Setup/[action]` (AllowAnonymous)

| Method | Action | Body |
|--------|--------|------|
| GET | Status | — |
| POST | TestDatabase | JSON: `{ Server, Port, Database, User, Password }` |
| POST | TestRedis | JSON: `{ Connection, InstanceName }` |
| POST | Install | JSON: `SetupRequest` (see [AI_Setup_Wizard](AI_Setup_Wizard.md)) |

## ApiController `/api/[action]` (JwtApi)

| Method | Action | Params | Note |
|--------|--------|--------|------|
| POST | Upload | `file` IFormFile | AllowAnonymous |
