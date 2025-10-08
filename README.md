# EasyAdmin

## 🌈 简介

> `EasyAdmin`是基于`Web API(.NET 8.0)`+`React`实现的通用后台管理系统。

- 后端技术栈：`Web API(.NET 8.0)` + `DDD` + `Dapper`
- 前端技术栈：`React` + `Ant-Design` + `Vite`

## ⭐ 开源

- 后端项目: https://github.com/Sean-Lu/EasyAdmin-Api
- 前端项目: https://github.com/Sean-Lu/EasyAdmin-Web

## 💖 内置功能

- [x] 租户管理：多租户
- [x] 用户管理
- [ ] 角色管理
- [ ] 部门管理
- [ ] 岗位管理
- [x] 菜单管理
- [ ] 字典管理
- [x] 参数管理：全局参数配置
- [ ] 应用管理
- [ ] 缓存管理
- [ ] 定时任务
- [x] 任务管理
- [x] 文件管理：上传文件
- [x] 待办事项：To-do List
- [x] 签到：日常打卡
- [x] 日报：今日工作\明日计划
- [ ] 周报：本周工作\下周计划
- [ ] 月报：本月工作\下月计划
- [x] 加解密：加密解密工具
- [x] 登录日志
- [x] 操作日志
- [ ] ......

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
5. 浏览器访问前端页面： http://127.0.0.1:9090 或 http://sean.easyadmin.com:9090

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