# 本地开发

## 后端

后端项目位于：

```text
apps/backend/csAPI/SIFS/
```

后端技术栈：

- ASP.NET Core
- .NET 8
- Entity Framework Core
- MySQL
- JWT
- Swagger

后端环境配置位于：

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
```

`AppUrlOptions__BaseUrl` 用于生成前端和算法服务可访问的文件 URL。后端遇到 `localhost`、`127.0.0.1`、`::1` 这类本地绝对地址时，会按该配置改写域名和端口，避免 Docker 或远程部署时把不可访问的本机地址暴露出去。

常用命令：

```powershell
cd apps/backend/csAPI/SIFS
dotnet restore
dotnet build
dotnet run --urls http://localhost:5021
```

## 前端

用户端：

```powershell
cd apps/frontend/web
npm install
npm run dev
```

管理端：

```powershell
cd apps/frontend/web-op
npm install
npm run dev
```

默认访问地址：

- 用户端：`http://localhost:5173`
- 管理端首页：`http://localhost:5174/admin/dashboard`
- 管理端登录：`http://localhost:5174/admin/login`
- 后端 Swagger：`http://localhost:5021/swagger`
- FLDCF Health：`http://127.0.0.1:8000/health/fldcf`
- FECDNet Health：`http://127.0.0.1:8001/health/fecdnet`

## 本地脚本

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

## 初始化数据

主要 SQL 位于：

```text
apps/backend/csAPI/SIFS/Scripts/init/
deploy/docker/mysql/init/
```

RBAC 初始化必须执行。开发测试账号 SQL 仅供本地测试使用，不建议在生产环境执行。
