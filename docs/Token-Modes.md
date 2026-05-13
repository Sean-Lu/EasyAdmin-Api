# Token 模式说明文档

## 概述

本项目支持三种 Token 过期模式，可通过配置文件灵活切换：

| 模式         | 配置值                                                 | 说明                               |
| ---------- | --------------------------------------------------- | -------------------------------- |
| **固定过期**   | `TokenMode: Single` + `UseSlidingExpiration: false` | 原有的默认模式，JWT 到期后自然失效              |
| **滑动过期**   | `TokenMode: Single` + `UseSlidingExpiration: true`  | 用户活跃时自动刷新 token                  |
| **双Token** | `TokenMode: Refresh`                                | 使用 AccessToken + RefreshToken 组合 |

***

## 一、Token 模式详解

### 1.1 固定过期模式（默认）

```json
{
  "Jwt": {
    "TokenMode": "Single",
    "UseSlidingExpiration": false,
    "Expired": 360
  }
}
```

**工作原理：**

- 登录时返回单个 `accessToken`
- Token 到期后（如 360 分钟）自动失效
- 用户需重新登录获取新 token
- **优点**：简单，无需额外状态管理
- **缺点**：用户长时间操作可能突然被踢下线

### 1.2 滑动过期模式

```json
{
  "Jwt": {
    "TokenMode": "Single",
    "UseSlidingExpiration": true,
    "SlidingExpirationThreshold": 30,
    "Expired": 360
  }
}
```

**工作原理：**

- 登录时返回单个 `accessToken`
- 后端中间件检测 token 剩余有效期
- 当剩余时间小于阈值（如 30 分钟）时自动生成新 token
- 通过 `X-New-Token` 响应头发送给前端
- 前端拦截器自动更新本地 token

**时序图：**

```
用户请求 → 中间件检测 → 剩余时间<阈值? → 生成新token → 返回 X-New-Token 头
              ↓                         ↓
           正常放行                 前端更新token
```

### 1.3 双Token模式（推荐）

```json
{
  "Jwt": {
    "TokenMode": "Refresh",
    "Expired": 30,
    "RefreshTokenExpired": 10080
  }
}
```

**工作原理：**

- 登录时返回 `accessToken`（短期，如 30 分钟） + `refreshToken`（长期，如 7 天）
- `accessToken` 用于日常请求认证
- `refreshToken` 存储在 Redis 中，用于获取新的 `accessToken`
- 当 `accessToken` 过期（401）时，前端自动使用 `refreshToken` 刷新

**流程图：**

```
登录 → accessToken(30min) + refreshToken(7天) → Redis存储refreshToken
    ↓
正常请求 → accessToken认证 → 成功
    ↓
accessToken过期(401) → 使用refreshToken获取新token → 更新本地token → 重试请求
    ↓
refreshToken过期 → 跳转到登录页
```

***

## 二、Token 模式对比

| 特性       | 固定过期      | 滑动过期             | 双Token                |
| -------- | --------- | ---------------- | --------------------- |
| Token 数量 | 1         | 1                | 2                     |
| 自动刷新     | ❌         | ✅（后端主动）          | ✅（前端自动）               |
| 服务器状态    | 无状态       | Redis 存储当前 token | Redis 存储 refreshToken |
| 退出登录     | 黑名单 token | 黑名单 token        | 撤销所有 refreshToken     |
| 安全性      | 中         | 中                | 高                     |
| 复杂度      | 低         | 中                | 高                     |

***

## 三、Token 模式验证

| 模式                                | 登录                                                  | Token 刷新                          | Token 失效             | 退出登录              | 状态 |
| --------------------------------- | --------------------------------------------------- | --------------------------------- | -------------------- | ----------------- | -- |
| **固定过期** `Single + Sliding=false` | 返回 accessToken，无 refreshToken                       | 不支持（自然过期）                         | 401 → 跳转登录           | 黑名单 accessToken   | ✅  |
| **滑动过期** `Single + Sliding=true`  | 返回 accessToken，Redis 存储                             | 中间件自动生成新 token → `X-New-Token` 头  | 401 → 跳转登录           | 黑名单 accessToken   | ✅  |
| **双Token** `Refresh`              | 返回 accessToken + refreshToken，Redis 存储 refreshToken | 前端拦截器自动用 refreshToken 换新对（支持并发去重） | 401 → 自动刷新 → 失败则跳转登录 | 撤销所有 refreshToken | ✅  |

***

## 四、配置项说明

| 配置项                          | 类型     | 默认值      | 说明                                            |
| ---------------------------- | ------ | -------- | --------------------------------------------- |
| `TokenMode`                  | string | `Single` | Token 模式：`Single`（单Token） / `Refresh`（双Token） |
| `UseSlidingExpiration`       | bool   | `false`  | 是否启用滑动过期（仅 Single 模式有效）                       |
| `SlidingExpirationThreshold` | int    | `30`     | 滑动过期阈值（分钟），token 剩余时间小于此值时自动刷新                |
| `Expired`                    | int    | `30`     | AccessToken 过期时间（分钟）                          |
| `RefreshTokenExpired`        | int    | `10080`  | RefreshToken 过期时间（分钟），默认 7 天，仅双Token模式有效      |

***

## 五、前端实现要点

### 5.1 响应拦截器处理

前端需要处理两种自动刷新场景：

```typescript
// 1. 滑动过期：接收 X-New-Token 响应头
const newToken = headers["x-new-token"];
if (newToken) {
    store.dispatch(setToken(newToken));
}

// 2. 双Token：401 时自动刷新
if (response.status === 401 && refreshToken) {
    const refreshResult = await refreshTokenApi(refreshToken);
    store.dispatch(setToken(refreshResult.data.accessToken));
    localStorage.setItem("refreshToken", refreshResult.data.refreshToken);
}
```

### 5.2 并发请求去重

双Token模式下需处理并发场景：

```typescript
let isRefreshing = false;
let failedRequests: Array<{ resolve, reject }> = [];

// 第一个请求触发刷新，其他请求加入等待队列
if (!isRefreshing) {
    isRefreshing = true;
    // 执行刷新...
    failedRequests.forEach(callback => callback.resolve(newAccessToken));
    failedRequests = [];
} else {
    // 加入等待队列
    return new Promise((resolve, reject) => {
        failedRequests.push({ resolve, reject });
    });
}
```

### 5.3 退出登录

```typescript
// 退出登录时需：
// 1. 调用 logoutApi 通知服务端
// 2. 清除本地 token
// 3. 清除本地 refreshToken
await logoutApi();
store.dispatch(setToken(""));
localStorage.removeItem("refreshToken");
```

***

## 六、安全注意事项

### 6.1 双Token模式安全要点

1. **RefreshToken 一次一用**：每次刷新后旧的 refreshToken 立即失效
2. **Access Token 有效期要短**：建议 15-30 分钟，减少泄露风险
3. **RefreshToken 存储**：前端存 `localStorage`，服务端存 Redis（带过期时间）

**问：refreshToken 配置的是"长期有效"（如 7 天），为什么还要"一次一用"？**

**答：这两点不矛盾！**

| 概念       | 说明                                         |
| -------- | ------------------------------------------ |
| **长期有效** | 表示如果不使用，refreshToken 最多可以保留 7 天            |
| **一次一用** | 表示一旦用它去刷新了，立即失效，同时会返回一个**新的 refreshToken** |

**完整刷新流程示例：**

```
第 1 次登录 → refreshToken1（有效期7天）
    ↓
用 refreshToken1 刷新 → accessToken2 + refreshToken2（7天） → refreshToken1 失效
    ↓
用 refreshToken2 刷新 → accessToken3 + refreshToken3（7天） → refreshToken2 失效
    ↓
...
```

**为什么要这样设计？**

这是一个安全措施：

| 场景              | 不使用"一次一用"           | 使用"一次一用"                 |
| --------------- | ------------------- | ------------------------ |
| refreshToken 泄露 | 攻击者可以无限刷新，直到 7 天后过期 | 攻击者只能用一次，下次刷新就失败，可快速发现风险 |

**实际效果：**

- 只要用户保持活跃，refreshToken 可以无限期使用（每次刷新都会获得一个新的 7 天有效期 token）
- 如果用户 7 天未登录，refreshToken 自然过期，需重新登录

### 6.2 CORS 配置

滑动过期模式需要暴露自定义响应头：

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithExposedHeaders("X-New-Token", "Authorization"); // 关键配置
    });
});
```

**原因**：浏览器默认只允许 JS 读取 6 个"简单响应头"（Cache-Control、Content-Type 等），自定义头必须显式暴露。

***

## 七、推荐配置方案

### 方案一：普通管理后台

```json
{
  "Jwt": {
    "TokenMode": "Single",
    "UseSlidingExpiration": true,
    "SlidingExpirationThreshold": 30,
    "Expired": 360
  }
}
```

### 方案二：高安全要求场景

```json
{
  "Jwt": {
    "TokenMode": "Refresh",
    "Expired": 15,
    "RefreshTokenExpired": 1440
  }
}
```

### 方案三：简单演示系统

```json
{
  "Jwt": {
    "TokenMode": "Single",
    "UseSlidingExpiration": false,
    "Expired": 1440
  }
}
```

***

**文档版本**: v1.0\
**创建日期**: 2026-05-14\
**最后更新**: 2026-05-14\
**适用版本**: EasyAdmin v1.0+
