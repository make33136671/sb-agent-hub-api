# SbAgentHub OpenIM 集成设计说明

## 概述

本项目使用 OpenIM 作为即时通讯引擎，实现访客（ChatUser）与客服（Agent/Member）之间的实时聊天功能。系统分为三层集成：

1. **后端 (agent-api)** — 通过 OpenIM Server REST API 管理用户注册、Token 获取、系统消息发送
2. **管理端 (agent-admin)** — 客服工作台通过 JS SDK (`@openim/client-sdk`) 建立 WebSocket 连接，实时收发消息
3. **Widget 端 (agent-widget-vue)** — 访客通过 JS SDK 建立 WebSocket 连接，与客服聊天

## 架构图

```
┌──────────────────────────────────────────────────────────────┐
│                      OpenIM Server                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  REST API    │  │  WebSocket   │  │  Message Queue   │   │
│  │  :10002      │  │  :10001      │  │  (内部)          │   │
│  └──────┬───────┘  └──────┬───────┘  └──────────────────┘   │
└─────────┼─────────────────┼──────────────────────────────────┘
          │                 │
          │                 ├─── agent-admin (客服工作台)
          │                 │    └── @openim/client-sdk (JS SDK)
          │                 │        via @saibotianlong/openim-core
          │                 │
          │                 └─── agent-widget-vue (访客 Widget)
          │                      └── @saibotianlong/openim-core (JS SDK)
          │
          └─── agent-api (.NET 后端)
               ├── OpenImAuthService    (Token 管理)
               ├── OpenImUserService    (用户注册/更新)
               └── OpenImMessageService (系统消息发送)
```

## 用户体系映射

### OpenIM UserID 生成规则

| 用户类型 | OpenIM UserID 格式 | 生成器 | 示例 |
|---------|-------------------|--------|------|
| 系统管理员 | `admin_{adminId}` | `OpenImIdGenerator.GenerateAdminId()` | `admin_1` |
| 客服/站长 | `agent_{memberId}` | `OpenImIdGenerator.GenerateAgentId()` | `agent_5` |
| 聊天访客 | `chat_{guid}` | `OpenImIdGenerator.GenerateChatUserId()` | `chat_a1b2c3d4e5f6...` |
| OpenIM 管理员 | `imAdmin` | 配置文件 | `imAdmin` |

### 数据库字段对应

| 表 | 字段 | 说明 |
|----|------|------|
| `SB_Member` | `OpenImUserId` | 客服的 OpenIM UserID |
| `SB_ChatUser` | `ChatUserId` | 访客的 OpenIM UserID（即 `chat_xxx`）|

## 后端集成 (agent-api)

### 服务层架构

```
SbAgentHub.Services/
├── OpenImAuthService.cs      # Token 管理
├── OpenImUserService.cs      # 用户注册/更新/查询
├── OpenImMessageService.cs   # 系统消息发送
└── ChatAuthService.cs        # 聊天认证（组合以上服务）
```

### OpenImAuthService — Token 管理

**职责**：管理 OpenIM 的认证 Token

- `GetAdminTokenAsync()` — 获取管理员 Token（缓存 55 分钟，Token 有效期 1 小时）
- `GetUserTokenAsync(userId, platformId)` — 获取指定用户的 Token
- `InvalidateAdminToken()` — 清除管理员 Token 缓存

**调用的 OpenIM API**：
- `POST /auth/get_admin_token` — 获取管理员 Token
- `POST /auth/user_token` — 获取用户 Token

**缓存策略**：
```
管理员 Token → ICacheService（Redis/Memory）→ key: "openim:admin:token"，55 分钟过期
```

### OpenImUserService — 用户管理

**职责**：在 OpenIM 中注册和管理用户

- `RegisterUserAsync(userId, nickname, faceURL)` — 注册用户（幂等，已存在返回 true）
- `UpdateUserInfoAsync(userId, nickname, faceURL)` — 更新用户信息
- `CheckUserExistsAsync(userId)` — 检查用户是否存在
- `RegisterAndGetTokenAsync(userId, nickname, faceURL, platformId)` — 注册并获取 Token（组合操作）

**调用的 OpenIM API**：
- `POST /user/user_register` — 注册用户
- `POST /user/update_user_info` — 更新用户信息
- `POST /user/get_users_info` — 查询用户信息

**错误码处理**：
- `errCode == 0` → 成功
- `errCode 11001-11099` → 用户已存在，视为成功

### OpenImMessageService — 消息服务

**职责**：后端直接发送系统消息

- `SendTextMessageAsync(fromUserId, toUserId, text)` — 发送文本消息
- `SendWelcomeMessageAsync(agentImUserId, chatUserImUserId, welcomeMessage)` — 发送欢迎语

**调用的 OpenIM API**：
- `POST /msg/send_msg` — 发送消息

**消息类型**：
| contentType | 含义 |
|-------------|------|
| 101 | 文本消息 |
| 102 | 图片消息 |
| 110 | 自定义消息 |

### ChatAuthService — 聊天认证流程

**访客认证流程** (`VisitorAuthAsync`)：

```
1. 验证 PlatformKey → 获取平台信息
2. 查找/创建 ChatUser
   ├── 按 ClientId 查找
   ├── 按 ExternalUserId 查找
   └── 都未找到 → 新建用户（生成 chat_xxx ID）
3. 在 OpenIM 注册用户 + 获取 Token
4. 分配客服（AgentAllocationService）
5. 记录登录日志
6. 新用户 + 有客服 → 发送欢迎语
7. 返回认证信息（IM Token、客服信息、平台信息）
```

**Token 刷新** (`RefreshImTokenAsync`)：
```
ChatUserId → OpenIM GetUserToken → 返回新 Token
```

**客服 IM Token** (`GetAgentImTokenAsync`)：
```
MemberId → 查 Member 表 → OpenImUserId
  └── 为空 → 自动生成 agent_xxx + 注册 OpenIM
→ 获取 Token → 返回
```

## 管理端集成 (agent-admin 客服工作台)

### SDK 选择

管理端使用 **JS SDK** (`@openim/client-sdk@^3.8.3`)，通过 `@saibotianlong/openim-core` 封装层调用。
通过 `@saibotianlong/openim-core` 封装层调用，JS SDK 更轻量、部署更简单。

### 模块结构

```
apps/web-antd/src/
├── modules/openim/
│   ├── config.ts          # OpenIM 核心实例单例工厂
│   ├── useAgentIM.ts      # 客服端 IM composable（模块级共享状态）
│   └── index.ts           # barrel export
└── views/workbench/
    ├── store.ts           # 双条件状态模型 + IM 连接管理
    ├── StatusSwitch.vue   # 状态切换（连接中动画 + 访客统计）
    ├── ChatPanel.vue      # 实时聊天面板
    ├── MessageBubble.vue  # 消息气泡组件（文本/图片）
    ├── VisitorList.vue    # 访客列表（搜索 + 刷新）
    ├── UserDetail.vue     # 用户详情
    └── Index.vue          # 工作台入口（自动恢复 IM）
```

### 环境变量

```bash
# .env.development
VITE_OPENIM_API_URL=http://192.168.10.231:10002
VITE_OPENIM_WS_URL=ws://192.168.10.231:10001
```

### 双条件在线状态模型

客服的有效在线状态由两个条件共同决定：

```
用户期望状态 (userDesiredStatus)  ─┐
                                   ├→ effectiveOnlineStatus (computed)
IM 连接状态 (imConnected)        ─┘
```

- `effectiveOnlineStatus >= 1` 仅当 `userDesiredStatus >= 1 && imConnected === true`
- IM 断连时自动 watch → 通知后端回退到离线

### 连接流程

```
用户点击"在线"
→ store.updateStatus(1)
  → store.goOnline(1)
    [UI: 蓝色脉冲 + "切换中..."]
    → getAgentImToken(memberId)     // 后端获取 IM Token
    → imLogin({ imUserId, token })  // JS SDK 连接 WebSocket
      → connection:success 事件
        → imConnected = true
        → effectiveOnlineStatus = 1
        [UI: 绿点 + "在线"]
    → postUpdateStatus(1)           // 通知后端
```

### useAgentIM composable

模块级共享状态（ref 定义在函数外，跨组件共享）：

| 状态 | 类型 | 说明 |
|------|------|------|
| `isConnected` | `ref<boolean>` | IM 是否已连接 |
| `isConnecting` | `ref<boolean>` | 是否正在连接 |
| `imUserId` | `ref<string \| null>` | 当前 IM 用户 ID |
| `messages` | `ref<Message[]>` | 当前对话消息 |
| `conversations` | `ref<Conversation[]>` | 会话列表 |

| 方法 | 说明 |
|------|------|
| `login(tokenData)` | 连接 OpenIM |
| `logout()` | 断开连接，重置状态 |
| `loadHistoryWithUser(userID, count)` | 加载与某用户的聊天记录 |
| `sendText(recvID, text)` | 发送文本消息 |

### 事件监听

```typescript
core.on('connection:connecting')  → isConnecting = true
core.on('connection:success')     → isConnected = true, isConnecting = false
core.on('connection:failed')      → isConnected = false, isConnecting = false
core.on('kicked:offline')         → isConnected = false
core.on('token:expired')          → isConnected = false
core.on('message:received')       → 追加到 messages
core.on('conversation:changed')   → 更新 conversations
```

### 状态切换 UI

| 条件 | 显示 | 视觉效果 |
|------|------|---------|
| `statusSwitching \|\| isConnecting` | "连接中..."/"断开中..."/"切换中..." | 蓝色(#1677ff)脉冲动画 |
| `effectiveOnlineStatus >= 1` | "在线"/"忙碌"/"离开" | 对应颜色圆点 |
| `effectiveOnlineStatus === 0` | "离线" | 红色圆点 |
| IM 断连 + 用户期望在线 | "离线" + 警告 | 橙色感叹号 "IM 连接断开" |

### 相关 API

| 接口 | 方法 | 说明 |
|------|------|------|
| `/Admin/Auth/GetAgentImToken` | GET | 获取客服 IM Token |
| `/Admin/Kefu/UpdateStatus` | POST | 更新客服在线状态 |
| `/Admin/Kefu/MyStatus` | GET | 获取当前客服状态 |
| `/Admin/Kefu/MyVisitors` | GET | 获取我的访客列表 |

## 前端 Widget 集成 (agent-widget-vue)

### SDK 初始化

```typescript
// config/index.ts — apiAddr/wsAddr 为可选，由服务端下发
{
  platformID: 5,                      // Web 平台
  logLevel: 2,                        // Error 级别
  baseURL: 'http://your-api-server:5153',
  platformKey: 'your-platform-key',
}
```

> **注意**：`apiAddr` 和 `wsAddr` 不再需要客户端配置，由认证接口 (`GetImToken` / `GetAgentImToken`) 在响应中通过 `imConfig` 字段下发。客户端也可在初始化时传入作为 fallback。

### 连接流程

```
Widget 打开
  → chatAuth.getImToken(platformKey, clientId, ...)
  → 获得 { chatUserId, token, imConfig: { apiUrl, wsUrl }, agent, ... }
  → core.updateImAddrs(imConfig.apiUrl, imConfig.wsUrl)  // 动态注入 IM 地址
  → core.login(chatUserId, token)
    → sdk.login({ userID, token, apiAddr, wsAddr, platformID })
  → 连接成功，开始收发消息
```

### 事件监听

```typescript
// composables/useOpenIM.ts 注册的事件
sdk.on(CbEvents.OnConnecting)        → 连接中
sdk.on(CbEvents.OnConnectSuccess)    → 连接成功
sdk.on(CbEvents.OnConnectFailed)     → 连接失败
sdk.on(CbEvents.OnKickedOffline)     → 被踢下线
sdk.on(CbEvents.OnUserTokenExpired)  → Token 过期 → 自动刷新
sdk.on(CbEvents.OnRecvNewMessage)    → 收到新消息
sdk.on(CbEvents.OnRecvNewMessages)   → 批量收到新消息
sdk.on(CbEvents.OnConversationChanged) → 会话变更
```

### 消息收发

**发送文本**：
```typescript
sdk.createTextMessage(text)
  → sdk.sendMessage({ message, recvID, groupID })
```

**发送图片**：
```typescript
// 方式 1：通过 URL（已上传到服务器）
sdk.createImageMessageByURL({ sourcePicture, bigPicture, snapshotPicture })
  → sdk.sendMessageNotOss({ message, recvID, groupID })

// 方式 2：降级为自定义消息
sdk.createCustomMessage({ data: JSON.stringify({ type: 'image', url, ... }) })
  → sdk.sendMessageNotOss(...)
```

**获取历史消息**：
```typescript
sdk.getAdvancedHistoryMessageList({
  conversationID,
  count: 20,
  startClientMsgID: '',  // 首次为空，加载更多时传最早消息 ID
  lastMinSeq: 0
})
```

### 自定义系统消息

Widget 端发送的系统消息（不在 UI 中展示）：

| type | 用途 | 发送时机 |
|------|------|---------|
| `visitor_online` | 访客上线通知 | Widget 连接成功 |
| `visitor_offline` | 访客离线通知 | Widget 关闭 |
| `user_info_updated` | 用户信息变更 | 更新昵称/头像 |

这些消息通过 `createCustomMessage` 创建，`extension` 中标记 `{ systemMessage: true, notDisplay: true }`。

在 `useOpenIM.ts` 中通过 `isSystemMessage()` 过滤，不展示在聊天列表中。

### Fetch 拦截器

OpenIM SDK 内部使用 `fetch` 调用 OpenIM API，部分响应中的数组字段可能为 `null`（SDK 期望 `[]`）。`services/openIM.ts` 中安装了全局 fetch 拦截器：

```typescript
// 仅拦截发往 OpenIM apiAddr 的请求
// 将已知数组字段从 null 修正为 []
const NULL_ARRAY_KEYS = new Set([
  'insert', 'delete', 'update',
  'friendsInfo', 'blackList',
  'groups', 'groupMembers',
  'conversationElems', 'conversations',
  'messageList', 'users',
])
```

### Token 自动刷新

```typescript
// composables/useChatAuth.ts
registerTokenRefreshCallback(async () => {
  const newToken = await chatAuth.refreshImToken(chatUserId)
  await openIMService.initOpenIM(chatUserId, newToken)
})
```

当 SDK 触发 `OnUserTokenExpired` 事件时，自动调用后端刷新 Token 并重新登录。

### 会话持久化

```typescript
// localStorage 存储
{
  chatUserId: string,
  imToken: string,
  agentImUserId: string,
  expireTime: number,      // Token 过期时间戳
  timestamp: number         // 存储时间
}
```

恢复策略：Token 剩余有效期 > 5 分钟时复用，否则重新认证。

## 配置说明

### 后端 appsettings.json

```json
{
  "OpenIM": {
    "ApiUrl": "http://localhost:10002",
    "WsUrl": "ws://localhost:10001",
    "Secret": "openIM123",
    "AdminUserId": "imAdmin"
  }
}
```

> **IM 地址服务端下发**：后端在 `GetImToken` 和 `GetAgentImToken` 响应中自动携带 `imConfig: { apiUrl, wsUrl }`，客户端无需单独配置 IM 地址。这样做的好处：
> - 一处配置，多端生效 — IM 地址变更只需改 appsettings.json
> - 环境切换简单 — dev/staging/prod 的 IM 地址由服务端自动下发
> - 灵活调度 — 后续可根据用户区域、负载返回不同 IM 节点

### Widget 初始化参数

```typescript
// apiAddr/wsAddr 不再需要传入，由服务端下发
createWidget('#chat-widget', {
  baseURL: 'http://your-api-server:5153',
  platformKey: 'your-platform-key',
  platformID: 5,  // Web
  logLevel: 2,    // Error
})
```

> 如果仍需本地覆盖（如调试），可传入 `apiAddr`/`wsAddr`，优先级：**服务端下发 > 初始化传入**。

### PlatformID 对照

| 值 | 平台 |
|----|------|
| 1 | iOS |
| 2 | Android |
| 3 | Windows |
| 4 | macOS |
| 5 | Web |
| 7 | MiniProgram |
| 8 | Linux |
| 9 | Admin |

## 注意事项

1. **IM 地址服务端下发**：客户端不再需要配置 `apiAddr`/`wsAddr`，由 `GetImToken` / `GetAgentImToken` 响应中的 `imConfig` 字段自动下发。SDK 通过 `core.updateImAddrs()` 在登录前动态注入
2. **跨域配置**：OpenIM Server 的 API 和 WebSocket 端口需配置 CORS
3. **Token 有效期**：默认 1 小时，后端缓存 55 分钟，前端 5 分钟安全余量
4. **TypeScript 限制**：项目启用 `erasableSyntaxOnly`，不能使用 `enum`，改用 `as const` 对象 + 类型联合
5. **Fetch 拦截器**：全局安装，仅影响 OpenIM API 请求，不影响其他 HTTP 请求
6. **系统消息过滤**：`isSystemMessage()` 在消息列表中过滤系统类消息，确保用户不可见
