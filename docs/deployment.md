# Docker 部署

当前 Docker 部署分为三种模式。

## 核心系统部署

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

Nginx 路径约定：

```text
/admin/                  管理端静态页面和前端路由
/admin/dashboard/hub     管理端 Dashboard 实时通知 Hub
```

核心系统部署时，算法 API 被视为外部服务。Docker 初始化 SQL 默认写入：

```text
FLDCF   -> http://host.docker.internal:8000/detect/fldcf
FECDNet -> http://host.docker.internal:8001/detect/fecdnet
```

如果算法部署在远程机器或第三方平台，请在管理端算法管理页面修改 `api_url`。

## 本地全量 Docker 部署

如果希望本地同时启动核心系统和两个算法容器：

```powershell
docker compose -f docker-compose.yml -f deploy/docker/compose/docker-compose.local-full.yml up -d --build
```

该模式会额外启动：

- FLDCF API：`http://localhost:8000`
- FECDNet API：`http://localhost:8001`

Nginx 使用本地全量配置：

```text
deploy/docker/nginx/nginx.local-full.conf
```

并提供算法代理路径：

```text
/algo/fldcf/
/algo/fecdnet/
```

## 独立算法 Docker 部署

FLDCF 独立部署：

```powershell
docker compose -f deploy/docker/compose/docker-compose.fldcf.yml up -d --build
```

FECDNet 独立部署：

```powershell
docker compose -f deploy/docker/compose/docker-compose.fecdnet.yml up -d --build
```

这种模式适合将算法服务部署在另一台机器上，或单独管理算法服务生命周期。部署后只需要把管理端算法配置中的 `api_url` 改成对应地址。

## 开发测试账号

Docker 初始化脚本 `deploy/docker/mysql/init/99-dev-test-users.sql` 会插入开发测试账号：

- `admin` / `Admin@123456`
- `user` / `User@123456`

不要在生产部署中包含该 SQL。

## 生产注意事项

启动前建议设置环境变量：

```powershell
$env:MYSQL_ROOT_PASSWORD="replace-me"
$env:JWT_SECRET_KEY="replace-with-a-long-secret"
$env:PUBLIC_BASE_URL="https://your-domain.example"
$env:HTTP_PORT="80"
$env:ALGO_TASK_WORKER_COUNT="2"
$env:ALGO_TASK_QUEUE_CAPACITY="1000"
```

如果宿主机没有安装 NVIDIA Container Toolkit，请从算法服务中移除 `gpus: all`。
