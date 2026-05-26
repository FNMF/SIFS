# Satellite Image Forensics System

Satellite Image Forensics System, 简称 SIFS，是一个面向卫星图像伪造检测的前后端系统。

这个顶层 README 只作为项目入口和文档索引使用。详细说明统一放在 [docs](docs/README.md) 目录中，避免部署、开发、架构说明分散在多个 README 里。

## 快速入口

- [项目概览](docs/project-overview.md)
- [本地开发](docs/development.md)
- [Docker 部署](docs/deployment.md)
- [算法接入](docs/algorithm-integration.md)
- [前端应用](docs/frontend.md)

## 代码入口

```text
apps/backend/csAPI/SIFS/     后端 ASP.NET Core API
apps/frontend/web/           用户端 Vue 应用
apps/frontend/web-op/        管理端 Vue 应用
deploy/docker/               Docker、Nginx、MySQL 初始化和算法镜像配置
docs/                        项目文档
```

## 常用地址

本地开发：

- 用户端：`http://localhost:5173`
- 管理端：`http://localhost:5174`
- Swagger：`http://localhost:5021/swagger`

Docker 网关：

- 用户端：`http://localhost/`
- 管理端：`http://localhost/admin/`
- Swagger：`http://localhost/swagger/`
