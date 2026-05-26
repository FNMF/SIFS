# 前端应用

项目包含两个 Vue 3 + Vite 前端应用。

## 用户端

路径：

```text
apps/frontend/web/
```

主要页面包含登录、注册、首页、上传、历史记录、任务详情和结果对比。

常用命令：

```powershell
cd apps/frontend/web
npm install
npm run dev
npm run build
npm run lint
```

## 管理端

路径：

```text
apps/frontend/web-op/
```

主要页面包含 Dashboard、任务管理、算法管理、算法健康状态、操作日志、用户角色管理、上传、历史记录、任务详情和结果对比。

常用命令：

```powershell
cd apps/frontend/web-op
npm install
npm run dev
npm run build
npm run lint
```

## 依赖和运行要求

两个前端应用都使用：

- Vue 3
- Vite
- Vue Router
- Pinia
- Element Plus
- Axios

Node 版本要求来自各自的 `package.json`：

```text
^20.19.0 || >=22.12.0
```
