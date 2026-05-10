# Satellite Image Forensics System

Satellite Image Forensics System, 简称 SIFS，是一个面向卫星图像伪造检测的前后端系统。项目当前包含用户端、管理端、后端 API、任务调度、算法服务接入、RBAC 权限、操作日志、任务审计、算法健康检查和 Dashboard 聚合能力。

## 项目简介

系统的核心设计是：**后端不硬编码算法类型和算法地址，算法配置由数据库 `algo_models` 管理**。算法服务可以是本机 Python API、独立 Docker 容器、远程服务器，或者第三方 HTTP API。

主要能力：

- 用户端上传卫星图像并创建检测任务。
- 后端根据数据库算法配置生成算法子任务。
- 任务调度支持内存队列、分区调度、幂等状态流转和失败原因记录。
- 管理端支持任务管理、算法管理、算法健康状态、操作日志、用户角色管理和 Dashboard。
- RBAC 权限控制覆盖后端接口和管理端菜单/按钮。
- 文件统一保存在 `apps/data`，按用户和业务类型隔离。
- FLDCF、FECDNet 等 Python 算法服务作为可独立部署的 API 接入。

## 目录结构

```text
apps/
  backend/
    csAPI/SIFS/          后端 ASP.NET Core API
    Reference/           第三方/封装算法代码，本仓库默认忽略
  frontend/
    web/                 用户端 Vue 前端
    web-op/              管理端 Vue 前端
  data/                  本地运行时文件目录
deploy/
  docker/
    compose/             Docker 部署模式说明和算法独立 compose
    mysql/init/          Docker MySQL 初始化 SQL
    nginx/               Nginx 配置
docker-compose.yml       核心系统 Docker Compose
data/local-scripts/      本地一键启动/停止脚本，默认不提交
```

## 后端环境配置

后端配置位于：

```text
apps/backend/csAPI/SIFS/.env
```

模板文件：

```text
apps/backend/csAPI/SIFS/.env.example
```

`.env` 不会提交到 Git。至少需要配置：

```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=sifs;User=root;Password=CHANGE_ME;
Jwt__Issuer=SIFS
Jwt__Audience=WebClient
Jwt__SecretKey=CHANGE_ME_USE_A_LONG_RANDOM_SECRET
Jwt__ExpiresMinutes=25
AppUrlOptions__BaseUrl=http://localhost:5021
AppUrlOptions__PyBaseUrl=http://localhost:5021
```

## Docker 部署

当前 Docker 部署分为三种模式。

### 1. 核心系统部署

默认 `docker-compose.yml` 只启动核心系统：

- MySQL
- 后端 API
- 用户端前端
- 管理端前端
- Nginx

不会默认构建或启动 `Reference` 下的 Python 算法服务。

```powershell
docker compose up -d --build
```

默认访问：

- 用户端：`http://localhost/`
- 管理端：`http://localhost/admin/`
- Swagger：`http://localhost/swagger/`

核心系统部署时，算法 API 被视为外部服务。Docker 初始化 SQL 默认写入：

```text
FLDCF   -> http://host.docker.internal:8000/detect/fldcf
FECDNet -> http://host.docker.internal:8001/detect/fecdnet
```

如果算法部署在远程机器或第三方平台，请在管理端算法管理页面修改 `api_url`。

### 2. 本地全量 Docker 部署

如果希望本地同时启动核心系统和两个算法容器：

```powershell
docker compose -f docker-compose.yml -f deploy/docker/compose/docker-compose.local-full.yml up -d --build
```

该模式会额外启动：

- FLDCF API：`http://localhost:8000`
- FECDNet API：`http://localhost:8001`

Nginx 也会使用本地全量配置：

```text
deploy/docker/nginx/nginx.local-full.conf
```

并提供算法代理路径：

```text
/algo/fldcf/
/algo/fecdnet/
```

### 3. 独立算法 Docker 部署

FLDCF 独立部署：

```powershell
docker compose -f deploy/docker/compose/docker-compose.fldcf.yml up -d --build
```

FECDNet 独立部署：

```powershell
docker compose -f deploy/docker/compose/docker-compose.fecdnet.yml up -d --build
```

这种模式适合将算法服务部署在另一台机器上，或者单独管理算法服务生命周期。部署后只需要把管理端算法配置中的 `api_url` 改成对应地址。

## 本地开发启动

本地脚本位于：

```text
data/local-scripts/
```

常用脚本：

```powershell
data/local-scripts/start-all.bat
data/local-scripts/stop-all.bat
data/local-scripts/restart-all.bat
```

默认本地访问：

- 用户端：`http://localhost:5173`
- 管理端：`http://localhost:5174`
- 后端 Swagger：`http://localhost:5021/swagger`
- FLDCF Health：`http://127.0.0.1:8000/health/fldcf`
- FECDNet Health：`http://127.0.0.1:8001/health/fecdnet`

## 初始化数据

主要 SQL 位于：

```text
apps/backend/csAPI/SIFS/Scripts/init/
deploy/docker/mysql/init/
```

其中 RBAC 初始化必须执行，开发测试账号 SQL 仅供本地测试使用，不建议生产环境执行。

## 注意事项

- `Reference/` 默认不提交，算法代码和权重可以独立管理。
- `apps/data/` 是运行时文件目录，生产环境需要规划磁盘容量和清理策略。
- 后端最终以数据库中的 `algo_models.api_url` 为准调用算法服务。
- 前端只做权限体验控制，后端 RBAC 才是最终安全边界。
