# EasyAdmin

## 🌈 简介

> `EasyAdmin`是基于`Web API(.NET 8.0)`+`React`实现的通用后台管理系统。

- 后端技术栈：`Web API(.NET 8.0)` + `DDD` + `Dapper`
- 前端技术栈：`React` + `Ant-Design` + `Vite`

## ⭐ 开源

- 后端项目: https://github.com/Sean-Lu/EasyAdmin-Api
- 前端项目: https://github.com/Sean-Lu/EasyAdmin-Web

## 💖 内置功能

### 🍺 内置账号

- **超级管理员**（拥有所有系统权限，包括租户配置）：superAdmin\superAdmin
- **管理员**（拥有租户内所有系统权限）：admin\admin
- **普通用户**（仅拥有普通用户权限，不能进行系统配置和管理）：user01\123456、user02\123456

### 🌻 内置菜单

- [x] **首页**：欢迎页面，登录后默认展示
- [x] **数据大屏**：智慧旅游可视化大数据展示平台（示例）

#### 🔐 系统管理

- [x] **租户管理**：支持多租户架构，实现租户数据隔离与独立管理
- [x] **用户管理**：用户信息维护，支持用户增删改查、状态管理、密码重置等
- [x] **角色管理**：角色权限配置，支持角色创建、编辑、删除及权限分配
- [x] **部门管理**：组织架构管理，支持树形结构展示和部门信息维护
- [x] **岗位管理**：岗位信息维护，支持岗位与部门、用户的关联管理
- [x] **行政区划**：行政区划管理，支持省/市/区三级树形结构展示和数据维护
- [x] **菜单管理**：系统菜单配置，支持动态菜单生成、权限控制和路由配置
- [x] **字典管理**：系统字典配置，用于维护系统常用枚举类型和选项值
- [x] **参数管理**：全局参数配置，支持系统级参数的动态配置和管理
- [x] **缓存管理**：系统缓存管理，支持系统内Redis缓存的查看、操作和监控
- [x] **定时任务**：定时任务配置，支持系统内定时任务的创建、编辑、删除、执行等功能
- [x] **文件管理**：文件上传下载，支持本地文件存储和阿里云 OSS 对象存储
- [x] **更新管理**：客户端更新管理，支持客户端自动检查更新和下载最新版本
- [x] **通知管理**：系统通知管理，支持系统内通知、短信通知、邮件通知等

#### 👤 个人中心

- [x] **我的消息**：个人消息中心，支持查看管理员发送的通知、未读提醒和消息已读管理
- [x] **我的笔记**：个人笔记管理，支持分类、标签、全文搜索、富文本图片、导出和密码打开保护
- [x] **我的分享**：个人分享管理，支持文件/笔记分享列表、链接复制、启停、配置和重新生成链接
- [x] **待办事项**：个人待办事项管理，支持待办事项的创建、编辑、删除和状态管理
- [x] **签到**：签到打卡管理，支持签到记录的查询、统计和管理
- [x] **日报**：个人日报管理，今日工作\明日计划
- [x] **周报**：个人周报管理，本周工作\下周计划
- [x] **月报**：个人月报管理，本月工作\下月计划

#### 🧰 工具

- [x] **代码生成**：支持数据库模式、代码解析模式、配置模式的代码生成功能
- [x] **百宝箱**：常用工具集合，提供JSON解析、URL编码/解码、二维码、随机决策器、抽奖等工具

#### 📋 日志管理

- [x] **登录日志**：用户登录记录，包括登录时间、IP地址、登录状态等信息
- [x] **操作日志**：系统操作日志，记录用户的关键操作行为

#### 📊 运行监控

- [x] **服务器监控**：查看 API 所在服务器的 CPU、内存、磁盘、网络和 .NET 进程信息

## 🌹 部署服务

### ☕ 本地部署

> 后端项目：

1. 运行web服务：

```
# 使用默认配置(appsettings.json)，默认监听端口：9001(http)、9002(https)
dotnet EasyAdmin.Web.dll

# 指定监听端口(需要注释掉appsettings.json里的Kestrel.Endpoints配置，否则指定的端口配置会被覆盖)：
dotnet EasyAdmin.Web.dll --urls http://*:9001
```

2. 浏览器访问后端接口： http://127.0.0.1:9001/swagger

> 前端项目：

```
# 1.安装依赖：

npm install

# 2.运行：

npm run dev

# 3.打包：

## 开发环境
npm run build:dev
## 测试环境
npm run build:test
## 生产环境
npm run build:prod
```

### ☕ IIS部署(Windows)

- Windows系统推荐通过IIS部署前后端服务：

类型 | 主机名 | 端口 | IP地址 | 备注
---|---|---|---|---
http | sean.easyadmin-api.com | 80 | * | 后端：通过域名访问
http |  | 8081 | * | 后端：通过IP+端口访问
---- |
http | sean.easyadmin.com | 80 | * | 前端：通过域名访问
http | | 8080 | * | 前端：通过IP+端口访问

- 设置`Hosts`(推荐使用[`SwitchHosts`](https://github.com/oldj/SwitchHosts)工具)：通过IP直接访问的话可以忽略这一步，主要是为了可以通过域名访问。

```
# EasyAdmin
127.0.0.1	sean.easyadmin.com
127.0.0.1	sean.easyadmin-api.com
```

- 浏览器访问后端接口：http://sean.easyadmin-api.com/swagger
- 浏览器访问前端页面：http://sean.easyadmin.com/
- 解决前后端跨域问题：

```
开发环境：使用Vite自带的proxy正向代理（vite.config.ts）
生产环境：使用Nginx反向代理
```

> 通过 `Nginx` 部署前端项目（设置开机自启）：

1. 下载 `Nginx`，修改配置文件：`\conf\nginx.conf`

```
    server {
        listen 9090;
        server_name  localhost;

        location / {
            proxy_pass http://127.0.0.1:8080;#前端
        }

        location /api/ {
            proxy_pass http://127.0.0.1:8081;#后端
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
```

2. 将 `Nginx` 注册为 Windows 服务：下载 [`Windows Service Wrapper`](https://github.com/winsw/winsw)(`WinSW-*.exe`)并重命名为：`nginx-service.exe`
3. 创建配置文件：`nginx-service.xml`

```xml
<service>
  <id>nginx</id>
  <name>Nginx Service</name>
  <description>High Performance Nginx Service</description>
  <logpath>logs</logpath>
  <log mode="roll-by-size">
    <sizeThreshold>10240</sizeThreshold>
    <keepFiles>8</keepFiles>
  </log>
  <executable>%BASE%\nginx.exe</executable>
  <startarguments>-g "daemon off;"</startarguments>
  <stopexecutable>%BASE%\nginx.exe</stopexecutable>
  <stoparguments>-s stop</stoparguments>
</service>
```

4. 将 `nginx-service.exe` 和 `nginx-service.xml` 文件移动到 `Nginx` 目录下，然后执行安装服务命令：`nginx-service.exe install`
5. 修改 `Nginx` 配置后，以管理员身份打开 `PowerShell`，在 `Nginx` 安装目录依次执行以下命令。只有配置检查成功后才重启服务：

```powershell
.\nginx.exe -t
.\nginx-service.exe restart
```

> `nginx-service.exe` 默认以 `LocalSystem` 账号运行 Nginx。普通终端执行 `nginx -s reload` 可能因无权访问服务进程创建的全局事件而出现 `OpenEvent(...) failed (5: Access is denied)`。通过管理员 PowerShell 重启 WinSW 服务可使新配置生效

6. 浏览器访问前端页面： http://127.0.0.1:9090 或 http://sean.easyadmin.com:9090

### ☕ Docker部署

> 为了实现前\后端可以通过容器名称互相访问（同一个网段下），需要创建自定义网络：

1. 使用 `docker network create easyadmin-net` 命令来创建一个自定义网络。
2. 使用 `docker network inspect easyadmin-net` 命令查看自定义网络的详细信息。
3. 运行前\后端容器时可以通过`--network easyadmin-net`指定容器使用的网络

> 后端项目：

1. 发布`EasyAdmin.Web`项目到指定文件夹
2. 构建`Docker`镜像：`docker build --no-cache -t easyadmin-api .`
3. 运行`Docker`容器：`docker run -d --name easyadmin-api -p 9001:9001 --privileged=true easyadmin-api`

```
# 默认配置(appsettings.json)是使用SQLite数据库。

# 通过环境变量指定数据库：
docker run -d --name easyadmin-api `
    -p 9001:9001 `
    --network easyadmin-net `
    -e ConnectionStrings__master="DataSource=192.168.6.151;Database=EasyAdmin;uid=root;pwd=12345!a;SslMode=none" `
    -e DatabaseSettings__DatabaseType="MySQL" `
    --privileged=true easyadmin-api

# 通过环境变量使用 Nacos 配置中心：
docker run -d --name easyadmin-api `
    -p 9001:9001 `
    --network easyadmin-net `
    -e NacosConfig__Enable=true `
    -e NacosConfig__ServerAddresses__0="http://127.0.0.1:8848/" `
    -e NacosConfig__UserName="nacos" `
    -e NacosConfig__Password="nacos" `
    -e NacosConfig__Namespace="9c41e209-1b76-4cd2-ba65-2e6fabfe5eb1" `
    --privileged=true easyadmin-api
```

4. 浏览器访问后端接口： http://127.0.0.1:9001/swagger

> 前端项目：

1. 本地构建打包：`npm run build:prod`
2. 构建`Docker`镜像：`docker build --no-cache -t easyadmin-web .`
3. 运行`Docker`容器：`docker run -d --name easyadmin-web -p 9002:9002 --network easyadmin-net easyadmin-web`
4. 浏览器访问前端页面： http://127.0.0.1:9002