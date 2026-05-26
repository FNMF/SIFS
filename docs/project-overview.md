# 项目概览

Satellite Image Forensics System, 简称 SIFS，是一个面向卫星图像伪造检测的前后端系统。项目当前包含用户端、管理端、后端 API、任务调度、算法服务接入、RBAC 权限、操作日志、任务审计、算法健康检查和 Dashboard 聚合能力。

## 核心设计

系统的核心设计是：后端不硬编码算法类型和算法地址，算法配置由数据库 `algo_models` 管理。

算法服务可以是：

- 本机 Python API
- 独立 Docker 容器
- 远程服务器
- 第三方 HTTP API

后端最终以数据库中的 `algo_models.api_url` 为准调用算法服务。

## 主要能力

- 用户端上传卫星图像并创建检测任务。
- 后端根据数据库算法配置生成算法子任务。
- 任务调度支持内存队列、分区调度、幂等状态流转和失败原因记录。
- 管理端支持任务管理、算法管理、算法健康状态、操作日志、用户角色管理和 Dashboard。
- RBAC 权限控制覆盖后端接口和管理端菜单、按钮。
- 文件统一保存在 `apps/data`，按用户和业务类型隔离。
- FLDCF、FECDNet 等 Python 算法服务作为可独立部署的 API 接入。

## 目录结构

```text
apps/
  backend/
    csAPI/SIFS/          后端 ASP.NET Core API
    Reference/           第三方或封装算法代码
  frontend/
    web/                 用户端 Vue 前端
    web-op/              管理端 Vue 前端
  data/                  本地运行时文件目录
deploy/
  docker/
    compose/             Docker 部署模式和算法独立 compose
    mysql/init/          Docker MySQL 初始化 SQL
    nginx/               Nginx 配置
docker-compose.yml       核心系统 Docker Compose
data/local-scripts/      本地一键启动、停止脚本
docs/                    项目文档
```

## 注意事项

- `Reference/` 中的算法代码和权重可以独立管理。
- `apps/data/` 是运行时文件目录，生产环境需要规划磁盘容量和清理策略。
- 前端只做权限体验控制，后端 RBAC 才是最终安全边界。
