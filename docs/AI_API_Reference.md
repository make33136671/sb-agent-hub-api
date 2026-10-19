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

## UserPreferencesController `/admin/UserPreferences/[action]`

| Method | Action | Params | Note |
|--------|--------|--------|------|
| GET | Get | — | 获取当前用户偏好设置 |
| POST | Save | form: `UserPreferences` | 保存用户偏好设置 |
| GET | GetSoundSettings | — | 获取声音设置（`SoundSettingsDto`） |
| POST | SaveSoundSettings | form: `SoundSettingsDto` | 保存声音设置 |
| GET | GetAvailableSounds | — | 获取可用提示音列表 |

**SoundSettingsDto 字段：**

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| newMessageEnabled | bool | true | 新消息提示音开关 |
| newMessageSound | string | "ding.mp3" | 新消息提示音文件 |
| newMessageVolume | int | 80 | 新消息提示音音量 (0-100) |
| systemNotificationEnabled | bool | true | 系统通知提示音开关 |
| systemNotificationSound | string | "dingdong.mp3" | 系统通知提示音文件 |
| systemNotificationVolume | int | 80 | 系统通知提示音音量 (0-100) |

## CustomerServiceConfigController `/admin/CustomerServiceConfig/[action]`

| Method | Action | Params | Note |
|--------|--------|--------|------|
| GET | GetConfig | — | 获取当前 Owner 的客服配置 |
| POST | SaveConfig | form/body: `CustomerServiceConfig` | 保存客服配置 |

**CustomerServiceConfig 关键字段：**

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| allocationRule | int | 0 | 分配规则：0=轮询 1=最少优先 2=随机 3=固定 |
| enableWorkTime | bool | false | 是否启用工作时间限制 |
| workTimeStart | string | "09:00" | 工作时间开始 |
| workTimeEnd | string | "18:00" | 工作时间结束 |
| enableQueue | bool | false | 是否启用排队功能 |
| maxConcurrentSessions | int | 50 | 最大并发会话数（排队时生效）|

## PendingDomainController `/admin/PendingDomain/[action]`

| Method | Action | Params | Note |
|--------|--------|--------|------|
| GET | List | query: `platformId`(optional), `field`, `order`, `page`, `pageSize` | 待审核域名列表（Admin 看全部，Owner 只看自己的） |
| GET | ListByPlatform | query: `platformId` | 指定平台的待审核域名 |
| POST | Approve | form: `id` | 批准域名（自动加入平台白名单） |
| POST | ApproveBatch | form: `ids` | 批量批准 |
| POST | Reject | form: `id` | 拒绝域名 |
| POST | RejectBatch | form: `ids` | 批量拒绝 |

**PendingDomain 字段：**

| 字段 | 类型 | 说明 |
|------|------|------|
| platformId | int | 所属平台 |
| ownerId | int | 所属站长 |
| domain | string | 请求来源域名 |
| requestCount | int | 请求次数 |
| lastIpAddress | string | 最近请求 IP |
| status | int | 0=待审核 1=已批准 2=已拒绝 |
| firstSeenTime | DateTime | 首次出现时间 |
| lastSeenTime | DateTime | 最后出现时间 |

**触发机制：** 当 ChatAuth 域名校验不匹配时，自动记录到 PendingDomain 表。管理员可在平台编辑页面审核并一键加入白名单。

## ApiController `/api/[action]` (JwtApi)

| Method | Action | Params | Note |
|--------|--------|--------|------|
| POST | Upload | `file` IFormFile | AllowAnonymous |
