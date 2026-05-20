# EasyAdmin 客户端自动更新服务 - 完整使用指南

---

## 一、系统概述

EasyAdmin 客户端自动更新服务是一套完整的桌面程序版本管理解决方案，包含以下组件：

| 组件 | 类型 | 说明 |
|------|------|------|
| EasyAdmin-Api | 现有 .NET 8 后端 | 提供更新服务 API、文件存储、跨版本差异计算 |
| EasyAdmin-Web | 现有 React 管理后台 | 更新管理页面（上传版本包、管理发布历史） |
| EasyUpdate.Core | 新增 .NET 类库 | 共享库，封装通用更新逻辑（版本检测、文件下载、校验、启动更新器） |
| AutoUpdater | 新增 .NET 控制台程序 | 独立子进程，负责在后台执行文件替换覆盖 |
| Demo | 新增示例桌面客户端 | 演示完整更新流程的参考实现 |

**核心设计理念**：EasyAdmin 负责「版本元数据管理 + 更新包存储 + 差异文件下发」，桌面客户端负责「版本检测 + 差异下载 + 自动替换」。EasyUpdate.Core 作为共享库，消除各客户端程序的重复开发。

---

## 二、架构总览

### 2.1 整体架构图

```
                    ┌──────────────────────────┐
                    │     EasyAdmin-Web         │
                    │   (React 管理后台)          │
                    │   "系统管理 → 更新管理"     │
                    │   上传zip / 管理版本 / 发布  │
                    └────────────┬─────────────┘
                                 │ JWT + Admin
                                 ▼
┌─────────────────────────────────────────────────────────┐
│                    EasyAdmin-Api (.NET 8)                │
│                                                         │
│  UpdateController : BaseApiController                   │
│  ┌──────────────────────────────────────────────────┐  │
│  │ [AllowAnonymous] GET  /api/update/check          │  │
│  │ [AllowAnonymous] GET  /api/update/manifest       │  │
│  │ [AllowAnonymous] GET  /api/update/downloadFile   │  │
│  │ [UserAuth]       POST /api/update/register       │  │
│  │ [UserAuth]       GET  /api/update/list           │  │
│  │ [UserAuth]       POST /api/update/setStatus      │  │
│  │ ...                                              │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  更新文件通过 IFileStorage 存储（支持本地/阿里云OSS）        │
│                                                         │
└─────────────────────┬───────────────────────────────────┘
                      │ HTTP/HTTPS
      ┌───────────────┼───────────────┐
      ▼                               ▼
┌──────────────┐              ┌──────────────┐
│   Demo.exe   │              │  其他客户端   │
│  (主程序)     │              │  (MainApp)    │
│              │              │              │
│ 引用:         │              │ 引用:         │
│ EasyUpdate   │              │ EasyUpdate   │
│ .Core.dll    │              │ .Core.dll    │
│              │              │              │
│ 启动 ────────▶              │              │
└──────┬───────┘              └──────────────┘
       │ 启动子进程
       ▼
┌──────────────┐
│ AutoUpdater  │
│   .exe       │
│ (独立子目录)  │
│ Updater/     │
│              │
│ 等待主程序退出 │
│ → 文件覆盖     │
│ → 启动主程序   │
└──────────────┘
```

### 2.2 客户端安装目录结构

```
%ProgramFiles%/EasyAdminDemo/
├── Demo.exe                        ← 主程序入口
├── Demo.dll                        ← 主程序依赖
├── Core.dll
├── config/
│   └── appsettings.json
├── version.json                    ← 嵌入资源（当前版本信息）
└── Updater/                        ← AutoUpdater 独立子目录
    ├── AutoUpdater.exe             ← 自动更新程序
    ├── EasyUpdate.Core.dll         ← 共享库
    └── ... (updater 自身依赖)
```

> **设计原则**：`Updater/` 子目录与主程序完全隔离，避免 DLL 版本冲突；更新 AutoUpdater 自身时只操作此目录。

---

## 三、EasyUpdate.Core 共享库

### 3.1 目的

避免每个客户端程序都重复开发一整套更新逻辑。所有桌面客户端只需引用此库即可获得完整更新能力。

### 3.2 依赖结构

```
Demo.exe ──────────────▶ EasyUpdate.Core.dll ◀────────────── AutoUpdater.exe
(版本检测/下载/启动更新器)    (共享逻辑)           (文件替换/自身更新)
```

### 3.3 核心命名空间

| 命名空间 | 核心类型 | 职责 |
|---------|---------|------|
| `EasyUpdate.Core.Models` | `VersionInfo`, `UpdateCheckResult`, `FileEntry`, `UpdateManifest`, `UpdateProgress` | 数据模型 |
| `EasyUpdate.Core.Client` | `UpdateChecker`, `UpdateDownloader`, `UpdateExecutor` | 核心业务逻辑 |
| `EasyUpdate.Core.Util` | `FileChecksumUtil`, `VersionUtil` | 工具类 |
| `EasyUpdate.Core.Config` | `UpdateConfig` | 配置管理 |

### 3.4 使用示例

```csharp
// 1. 加载配置
var config = new UpdateConfig
{
    ApiBaseUrl = "https://api.easyadmin.com",
    Platform = "win-x64",
    LocalInstallDir = AppContext.BaseDirectory,
    UpdaterDir = Path.Combine(AppContext.BaseDirectory, "Updater")
};

// 2. 检测更新
var checker = new UpdateChecker(config);
var result = await checker.CheckUpdateAsync();
if (!result.HasUpdate) return; // 无更新

// 3. 获取差异文件清单（跨版本自动计算）
var manifest = await checker.GetManifestAsync(result.LatestVersionCode);
var diffList = checker.CompareWithLocal(manifest); // 只下载真正变化的文件

// 4. 下载差异文件到临时目录
var tempDir = Path.Combine(Path.GetTempPath(), $"EasyUpdate_{result.LatestVersionName}");
var downloader = new UpdateDownloader(config);
await downloader.DownloadFilesAsync(diffList, tempDir, new Progress<UpdateProgress>(p =>
{
    Console.WriteLine($"下载进度: {p.Percentage}% ({p.DownloadedBytes}/{p.TotalBytes})");
}));

// 5. 启动更新器
var executor = new UpdateExecutor();
executor.LaunchAndExit(new UpdateLaunchOptions
{
    UpdaterDir = config.UpdaterDir,
    MainProcessId = Environment.ProcessId,
    TargetDir = config.LocalInstallDir,
    TempDir = tempDir,
    MainExeName = "Demo.exe",
    VersionName = result.LatestVersionName
});
// 主程序随后自行退出
```

---

## 四、更新流程详解

### 4.1 完整时序图

```
Demo.exe                    EasyAdmin-Api              AutoUpdater.exe
    │                            │                          │
    │ ① GET /api/update/check   │                          │
    │───────────────────────────▶│                          │
    │◄───────────────────────────│                          │
    │  hasUpdate: true           │                          │
    │                            │                          │
    │ ② GET /api/update/manifest│                          │
    │───────────────────────────▶│                          │
    │◄───────────────────────────│                          │
    │  filesToDownload: [...]    │                          │
    │  filesToDelete: [...]      │                          │
    │                            │                          │
    │ ③ 本地比对 + 筛选差异文件    │                          │
    │                            │                          │
    │ ④ GET .../downloadFile × N │                          │
    │───────────────────────────▶│                          │
    │◄───────────────────────────│                          │
    │  保存到临时目录             │                          │
    │                            │                          │
    │ ⑤ 校验 SHA256              │                          │
    │                            │                          │
    │ ⑥ 启动 AutoUpdater.exe    │                          │
    │ (命令行传参)               │                          │
    │──────────────────────────────────────────────────────▶│
    │                            │                          │
    │ ⑦ 主程序退出               │                          │
    │ ✗                          │                          │
    │                            │                    ⑧ 等待主进程退出
    │                            │                    ⑨ 自身更新检测（如需要）
    │                            │                    ⑩ 文件替换覆盖
    │                            │                    ⑪ 删除临时文件
    │                            │                    ⑫ 启动 Demo.exe
    │                            │                          │
    │ ⑬ Demo.exe 启动            │                          │
    │ 自动读取新版 version.json   │                          │
    │ 正常工作                    │                          │
```

### 4.2 步骤详解

#### 步骤①-②：版本检测与清单获取

```
GET /api/update/check?currentVersionCode=10000&platform=win-x64

响应 → 判断 hasUpdate
  ├── false → 无更新，正常启动
  └── true  → 继续获取清单

GET /api/update/manifest?currentVersionCode=10000&targetVersionCode=10100&platform=win-x64

响应 → 差异文件清单
  filesToDownload: [Core.dll, New.dll]
  filesToDelete:   [Old.dll]
  totalDownloadSize: 307200
```

#### 步骤③：本地文件比对

客户端拿到清单后，不需下载所有文件。EasyUpdate.Core 自动执行本地比对：

```
清单中的 Core.dll (SHA=ddd444)
  本地 Core.dll  SHA=aaa111  → 不同，加入下载队列 ✓

清单中的 New.dll (SHA=eee555)
  本地 New.dll   (不存在)     → 加入下载队列 ✓

清单中的 UI.dll (SHA=ccc333)
  本地 UI.dll    SHA=ccc333  → 相同，跳过 ✗
```

#### 步骤④-⑤：下载与校验

```
对下载队列中每个文件:
  GET /api/update/downloadFile?versionCode=10100&filePath=bin%2FCore.dll
  → 响应头 X-Checksum: sha256...
  → 下载完成后计算本地 SHA256 比对
  → 不匹配则重试（最多3次）
  → 保存到 %TEMP%/EasyUpdate_1.1.0/bin/Core.dll
```

#### 步骤⑥-⑦：启动更新器 + 主程序退出

```
AutoUpdater.exe
  --pid=12345
  --targetDir="C:\Program Files\EasyAdminDemo\"
  --tempDir="%TEMP%\EasyUpdate_1.1.0\"
  --mainExe="Demo.exe"
  --newUpdaterTempDir="%TEMP%\EasyUpdate_1.1.0\Updater\"  ← 可选，自身更新时使用
```

#### 步骤⑧-⑫：AutoUpdater 执行

详见下方「AutoUpdater 自身更新机制」章节。

---

## 五、AutoUpdater.exe 详解

### 5.1 命令行参数

| 参数 | 必需 | 说明 | 示例 |
|------|------|------|------|
| `--pid` | 是 | 主程序进程ID | `--pid=12345` |
| `--targetDir` | 是 | 主程序安装目录 | `--targetDir="C:\Program Files\MyApp\"` |
| `--tempDir` | 是 | 临时下载文件目录 | `--tempDir="%TEMP%\EasyUpdate_1.1.0\"` |
| `--mainExe` | 是 | 更新完成后启动的主程序文件名 | `--mainExe="Demo.exe"` |
| `--newUpdaterTempDir` | 否 | AutoUpdater 自身更新文件所在目录 | `--newUpdaterTempDir="%TEMP%\EasyUpdate_1.1.0\Updater\"` |
| `--version` | 否 | 目标版本名（用于日志） | `--version="1.1.0"` |

### 5.2 执行流程

```
AutoUpdater.exe 启动
    │
    ├── 1. 解析命令行参数
    │
    ├── 2. 等待主程序退出
    │     ├── 循环检测 Process.GetProcessById(pid) 是否已退出
    │     └── 最长 30 秒 → 超时则强制终止
    │
    ├── 3. 检查自身是否需要更新
    │     │
    │     ├── --newUpdaterTempDir 有值？→ 执行自身更新
    │     │     ├── 将当前 AutoUpdater.exe 重命名为 AutoUpdater.exe.old
    │     │     ├── 复制 newUpdaterTempDir 所有文件到 Updater/ 目录
    │     │     ├── 启动新的 Updater/AutoUpdater.exe（传相同参数，去掉 --newUpdaterTempDir）
    │     │     ├── 退出当前进程
    │     │     └── 新进程 → 删除 .old 文件 → 继续步骤4
    │     │
    │     └── 无值 → 自身无需更新，继续步骤4
    │
    ├── 4. 备份旧文件
    │     旧文件 → %APPDATA%/EasyAdmin/Backup/{timestamp}/
    │
    ├── 5. 替换文件
    │     遍历 tempDir 中所有文件：
    │       ├── 计算相对路径
    │       ├── 目标路径 = targetDir/相对路径
    │       ├── 若目标目录不存在 → 创建
    │       └── 复制覆盖 tempDir 中的文件到目标路径
    │
    ├── 6. 删除标记为"已移除"的文件
    │     读取 tempDir/incremental-manifest.json 中的 FilesToDelete 列表
    │     删除 targetDir 中对应的文件
    │
    ├── 7. 清理临时目录和遗留文件
    │     删除整个 tempDir
    │     清理上次自更新遗留的 .old 文件
    │
    ├── 8. 启动主程序
    │     Process.Start(targetDir/mainExe)
    │
    └── 9. AutoUpdater.exe 退出
```

### 5.3 自身更新机制

当 `--newUpdaterTempDir` 参数存在时，表示 Updater/ 目录中的文件（AutoUpdater.exe 及其依赖）也需要更新。

**原理**：Windows NTFS 允许重命名正在运行的 .exe 文件，但不能删除它。

```
执行流程:

  旧 AutoUpdater.exe [运行中]  →  rename → AutoUpdater.exe.old
  新 AutoUpdater.exe           ←  copy   ← newUpdaterTempDir
  Launch 新 AutoUpdater.exe → 旧进程退出 → 新进程删除 .old

关键约束:
  ✅ 运行中的 .exe 可以被重命名（NTFS 支持）
  ✅ 运行中的 .exe 可以被覆盖写入同名新文件
  ❌ 运行中的 .exe 不能被删除（但可先改名，新进程启动后再删旧的）
```

### 5.4 项目依赖

```
AutoUpdater.exe
  ├── EasyUpdate.Core  (引用共享库，复用校验/日志等工具)
  └── System.CommandLine (可选，用于命令行参数解析)
```

---

## 六、服务端更新包目录结构

### 6.1 文件存储布局

每个版本的更新文件通过 `IFileStorage` (LocalFile / AliyunOss) 存储：

```
{IFileStorage根路径}/update-packages/
├── {appCode}/                    ← 多客户端隔离
│   └── {platform}/
│       └── {versionCode}/
│           ├── Demo.exe
│           ├── Demo.dll
│           ├── Core.dll
│           ├── config/
│           │   └── appsettings.json
│           └── Updater/
│               ├── AutoUpdater.exe
│               └── EasyUpdate.Core.dll
```

例：
```
update-packages/
├── demo/
│   └── win-x64/
│       ├── 1/
│       │   ├── Demo.exe
│       │   ├── Core.dll
│       │   └── ...
│       ├── 2/
│       │   ├── Demo.exe              ← 与 v1 可能相同或不同
│       │   ├── Core.dll              ← 已变更
│       │   ├── New.dll               ← 新增
│       │   └── ...
│       └── 3/
│           └── ...
├── tool-a/
│   └── win-x64/
│       └── ...
└── tool-b/
    └── linux-x64/
        └── ...
```

> **关键**：每个版本存储的是「该版本的全量文件」，而非增量。增量差异在 API 调用时由服务端**实时计算**。这样做的好处是任何时候都能计算任意两个版本之间的差异，不会因为中间版本被删除而丢失差异计算能力。

### 6.2 存储方式

| 环境 | 存储后端 | 配置 |
|------|---------|------|
| 本地开发/测试 | `LocalFileStorage` | `appsettings.json` → `LocalFileStorage.RootPath` |
| 生产环境 | `AliyunOssStorage` (推荐) | 复用现有 `AliyunOss` 配置节 |

更新包文件**不**写入 `FileEntity` 表（那是面向业务文件管理的表，更新包有独立的 `UpdateFileEntity` 表）。

### 6.3 校验机制

| 层级 | 校验方式 | 生效时机 |
|------|---------|---------|
| **上传校验** | zip 文件完整性检查 | 管理员上传时 |
| **存储校验** | 每个文件计算 SHA256，写入 `UpdateFileEntity.Checksum` | 服务端解压 zip 时 |
| **下载校验** | 响应头 `X-Checksum` 携带 SHA256，客户端比对 | 客户端每次下载后 |
| **清单校验** | manifest 接口返回的 `checksum` 与下载文件 `X-Checksum` 一致 | 客户端比对清单和下载结果 |
| **传输加密** | HTTPS 强制 | 所有 API 调用 |

---

## 七、跨版本累积差异算法

### 7.1 原理

服务端为每个版本存储**全量文件清单**（`UpdateFileEntity` 表）。当客户端请求差异时，服务端直接比对两个版本的全量清单，输出差异结果。

### 7.2 示例

```
客户端: v1.0.0 → v1.2.0（跳过了 v1.1.0）

服务端:
  v1.0.0 全量清单                 v1.2.0 全量清单
  ┌──────────────────┐           ┌──────────────────┐
  │ Demo.exe  SHA=aaa│           │ Demo.exe  SHA=ddd│  → 不同，需下载
  │ Core.dll  SHA=bbb│           │ Core.dll  SHA=eee│  → 不同，需下载
  │ UI.dll    SHA=ccc│           │ UI.dll    SHA=ccc│  → 相同，跳过 ✗
  │ Old.dll   SHA=iii│           │ (不存在)         │  → 需删除
  └──────────────────┘           │ New.dll   SHA=fff│  → 新增，需下载
                                 └──────────────────┘

结果: 仅需下载 Demo.exe, Core.dll, New.dll (3个文件)
```

> **即使 Core.dll 在 v1.1.0 和 v1.2.0 各被修改过一次，客户端也只需下载最终的 v1.2.0 版本一次。**

### 7.3 首次安装场景

首次安装（客户端没有任何本地文件）时：

- 客户端 `currentVersionCode=0`（表示首次安装）
- 请求 `GET /api/update/manifest?currentVersionCode=0&targetVersionCode=10000`
- 服务端检测到 `currentVersionCode=0` → 返回目标版本的全量文件清单
- 用户只需下载「当前最新版本的全量文件」，无需下载历史版本

实际上首次安装更简单的做法是让用户从官网下载完整安装包，安装包中已包含 `Updater/AutoUpdater.exe`，后续版本变更通过 AutoUpdater 自动更新。

---

## 八、后端 API 参考

所有接口定义于 `UpdateController : BaseApiController`，路由模板 `api/[controller]/[action]`。

### 8.1 客户端调用接口（匿名）

| 接口 | 方法 | URL | 说明 |
|------|------|-----|------|
| 检测更新 | GET | `/api/update/check` | 检测指定平台是否有新版本 |
| 获取清单 | GET | `/api/update/manifest` | 获取两个版本间的差异文件清单 |
| 下载文件 | GET | `/api/update/downloadFile` | 流式下载单个文件 |

#### GET /api/update/check

```
请求参数:
  currentVersionCode   int     当前客户端版本号    10000
  platform             string  平台标识            win-x64

响应:
  hasUpdate            bool    是否有可用更新
  latestVersionName    string  最新版本号          1.1.0
  latestVersionCode    int     最新内部版本号      10100
  changelog            string  更新日志(Markdown)
  forceUpdate          bool    是否强制更新
  publishTime          string  发布时间
```

#### GET /api/update/manifest

```
请求参数:
  currentVersionCode   int     客户端当前版本号    10000
  targetVersionCode    int     目标版本号          10100
  platform             string  平台标识            win-x64

响应:
  targetVersionName    string  目标版本号          1.1.0
  totalDownloadSize    long    需要下载的总大小(字节)
  filesToDownload[]    array   需要下载的文件列表
    ├── filePath       string  相对路径            bin/Core.dll
    ├── fileSize       long    文件大小(字节)
    ├── checksum       string  SHA256校验值
    └── action         string  add / update
  filesToDelete[]      array   需要删除的文件列表
    ├── filePath       string  相对路径
    └── action         string  delete
```

#### GET /api/update/downloadFile

```
请求参数:
  versionCode          int     版本号              10100
  filePath             string  文件相对路径(URL编码) bin%2FCore.dll

响应:
  Content-Type: application/octet-stream
  X-Checksum:  sha256hex...
  X-File-Path: bin/Core.dll
```

### 8.2 管理端调用接口（需认证）

| 接口 | 方法 | URL | 说明 |
|------|------|-----|------|
| 注册版本 | POST | `/api/update/register` | 上传 zip + 版本元信息 |
| 版本列表 | GET | `/api/update/list` | 分页查询 |
| 版本详情 | GET | `/api/update/detail` | 版本信息 + 文件清单 |
| 编辑版本 | POST | `/api/update/update` | 修改版本元信息 |
| 发布/回滚 | POST | `/api/update/setStatus` | 修改版本状态 |
| 删除版本 | POST | `/api/update/delete` | 删除版本 + 清理存储 |

#### POST /api/update/register

```
请求类型: multipart/form-data
字段:
  versionName            string  1.1.0
  versionCode            int     10100
  platform               string  win-x64
  changelog              string  Markdown更新日志
  isForceUpdate          bool    false
  minSupportedVersionCode int    10000
  file                   file    release-v1.1.0.zip

zip 包内部结构要求:
  release-v1.1.0.zip
  ├── Demo.exe
  ├── Core.dll
  ├── config/
  │   └── appsettings.json
  └── Updater/
      ├── AutoUpdater.exe
      └── EasyUpdate.Core.dll

服务端自动:
  1. 解压 zip
  2. 遍历所有文件 → 计算 SHA256
  3. 通过 IFileStorage 存储 → 记录 StoragePath
  4. 写入 UpdateFileEntity (全量清单)
  5. 创建 UpdateVersionEntity 记录
```

---

## 九、前端管理台使用说明

### 9.1 菜单位置

```
系统管理 → 更新管理
```

### 9.2 操作流程

#### 发布新版本

```
1. 构建客户端程序 (如 dotnet publish -c Release -r win-x64)

2. 查看输出目录，确认目录结构正确:
   publish/
   ├── Demo.exe
   ├── Core.dll
   ├── config/
   │   └── appsettings.json
   └── Updater/
       ├── AutoUpdater.exe
       └── EasyUpdate.Core.dll

3. 将 publish/ 目录打包为 .zip 文件
   (注意：zip 内部直接是文件列表，不要多一层 publish/ 目录)

4. 打开 EasyAdmin-Web → 系统管理 → 更新管理 → 新增版本

5. 填写:
   - 版本号:          1.1.0
   - 内部版本号:       10100
   - 目标平台:         win-x64
   - 是否强制更新:     否 (特殊情况勾选)
   - 最低支持版本号:   10000
   - 更新日志:         ## v1.1.0 \n - 新增功能 \n - 修复Bug
   - 上传更新包:       选择刚才打包的 .zip

6. 点击「提交并发布」

7. 服务端自动解压、计算SHA256、存储文件
   发布后，客户端下次检测更新即可获取到新版本
```

#### 回滚版本

```
1. 打开 EasyAdmin-Web → 系统管理 → 更新管理

2. 找到需要回滚到的版本 (如 v1.0.0)

3. 点击「发布」→ 该版本变为"已发布"状态
   同时最新版本自动变为"已回滚"状态

4. 客户端下次检测 → check 接口返回已回滚后的最新版本
   客户端此后的增量更新正常执行即可
```

---

## 十、Demo.exe 参考实现要点

### 10.1 版本号管理

在 Demo 项目中嵌入 `version.json` 作为嵌入资源（Embedded Resource）：

```json
{
  "versionName": "1.0.0",
  "versionCode": 10000,
  "buildTime": "2026-05-08T10:00:00Z"
}
```

- 构建前由脚本/CI自动更新此文件内容
- `EasyUpdate.Core.VersionInfo.Load()` 从嵌入资源读取

### 10.2 配置项

`appsettings.json` 示例：

```json
{
  "Update": {
    "AutoCheckOnStartup": false,
    "ApiBaseUrl": "https://api.easyadmin.com",
    "Platform": "win-x64"
  }
}
```

### 10.3 UI 交互流程

```
程序启动
  │
  ├── (若 AutoCheckOnStartup=true) → 后台检测更新
  │
  ├── 用户点击菜单 "检查更新" → 手动检测
  │
  ├── 检测到更新 →
  │     ┌─────────────────────────────┐
  │     │  🔔 发现新版本 v1.1.0       │
  │     │                             │
  │     │  ## 更新内容                │
  │     │  - 新增导出功能              │
  │     │  - 修复已知 Bug            │
  │     │                             │
  │     │  [稍后提醒]    [立即更新]    │
  │     └─────────────────────────────┘
  │
  ├── 用户点击 "立即更新" →
  │     ┌─────────────────────────────┐
  │     │  ⏳ 正在下载更新...         │
  │     │  ████████████░░░░░ 75%     │
  │     │  3.2 MB / 4.5 MB           │
  │     └─────────────────────────────┘
  │
  ├── 下载+校验完成 →
  │     ┌─────────────────────────────┐
  │     │  更新已就绪                  │
  │     │  程序将自动重启以完成更新     │
  │     │              [确定]          │
  │     └─────────────────────────────┘
  │
  └── 程序重启 → 自动加载新版本
```

---

## 十一、常见问题

### Q: 用户首次安装时如何获得更新能力？

首次安装包中就应包含 `Updater/AutoUpdater.exe` + `EasyUpdate.Core.dll`。用户从官网下载完整安装包，后续版本变更通过自动更新完成。

### Q: AutoUpdater.exe 自身怎么更新？

通过 NTFS 的文件重命名特性实现。AutoUpdater 运行时可以重命名自身为 `.old`，然后从临时目录复制新版本到 `Updater/`，启动新版 AutoUpdater，旧版退出，新版删除 `.old`。详见第五节「自身更新机制」。

### Q: 跨多个版本更新时，会不会重复下载同一个文件？

不会。服务端 `manifest` 接口直接对比 `currentVersionCode` 和 `targetVersionCode` 两个版本的全量清单，只返回最终差异。同一文件在中间版本多次变更，只需下载一次最终版本。

### Q: 更新包的 zip 内部结构有要求吗？

zip 根目录直接对应客户端的安装根目录。例如 `Demo.exe` 在 zip 的根目录，`config/appsettings.json` 在 zip 的 `config/` 子目录下。不要在 zip 中额外嵌套一层目录。

### Q: 支持同时管理多个平台吗？

支持。`UpdateVersionEntity.Platform` 字段区分平台（`win-x64`, `win-x86`, `linux-x64` 等），每个平台的更新包独立管理和下发。

### Q: 更新过程中断电或崩溃怎么办？

- 下载阶段的文件保存在临时目录（`%TEMP%`），不影响正常运行的程序
- AutoUpdater 在替换文件前可启用备份（将旧文件备份到 `%APPDATA%/EasyAdmin/Backup/`）
- 替换文件的最后一步才是覆盖 `Demo.exe`，在此之前所有操作可回滚
- 若 AutoUpdater 崩溃，用户重新启动主程序 → 再次检测 → 重新下载 → 重试
- 建议 AutoUpdater 在替换文件时记录日志，便于排查问题

---

## 十二、安全考虑

| 层面 | 措施 |
|------|------|
| 传输安全 | 所有 API 强制 HTTPS |
| 文件完整性 | 每个文件 SHA256 校验值记录于数据库，下载时通过 `X-Checksum` 响应头传递 |
| 客户端校验 | 下载完成后计算本地 SHA256 与响应头比对，不匹配则拒绝使用 |
| 接口限流 | 匿名接口（check/manifest/downloadFile）按 IP 限流，防止滥用 |
| 管理接口 | 上传/发布/删除等接口需要 JWT + 管理员角色 |
| 路径穿越防护 | 解压 zip 时校验文件名，拒绝包含 `../` 的恶意路径名 |
| 文件类型限制 | 仅允许 `.dll`, `.exe`, `.json`, `.xml`, `.config`, `.pdb`, `.png`, `.ico` 等白名单扩展名 |