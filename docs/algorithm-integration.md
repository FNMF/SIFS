# 算法接入说明

本文档说明外部算法服务需要如何与 SIFS 后端对接。核心原则是：后端只保存算法模型配置，并按统一 DTO 调用算法 HTTP 接口；算法服务必须兼容这个请求和响应结构。

## 调用方向

SIFS 后端作为 HTTP 客户端调用算法服务。

```text
SIFS backend -> POST algo_models.api_url -> Algorithm service
```

算法地址来自数据库表 `algo_models` 的 `api_url` 字段，可在管理端算法管理页面配置。

相关后端代码：

- `apps/backend/csAPI/SIFS/Infrastructure/External/AiService.cs`
- `apps/backend/csAPI/SIFS/Infrastructure/External/DetectionResult.cs`
- `apps/backend/csAPI/SIFS/Application/AlgoModels/AlgoModelCreateDto.cs`
- `apps/backend/csAPI/SIFS/Application/AlgoModels/AlgoModelUpdateDto.cs`

## 算法模型配置 DTO

管理端新增算法时，后端接收 `AlgoModelCreateDto`：

```json
{
  "name": "FLDCF",
  "apiUrl": "http://127.0.0.1:8000/detect/fldcf",
  "description": "Default FLDCF algorithm endpoint",
  "reservedJson": {
    "runtime": {
      "resource_pool": "default",
      "algorithm_concurrency": 1,
      "resource_pool_concurrency": 1,
      "timeout_seconds": 300
    }
  }
}
```

字段说明：

| 字段 | 必填 | 说明 |
| --- | --- | --- |
| `name` | 是 | 算法名称，会随算法调用 payload 一起传给算法服务。 |
| `apiUrl` | 是 | 算法检测接口地址，后端会对这个地址发起 `POST` 请求。 |
| `description` | 否 | 算法说明。 |
| `reservedJson` | 否 | 扩展配置，目前用于运行时调度参数。 |

更新算法时使用 `AlgoModelUpdateDto`，字段相同但都可以为空；只传需要修改的字段。

## 算法检测接口

算法服务必须提供一个可被后端访问的 HTTP `POST` 接口。接口路径不强制固定，但必须配置到 `algo_models.api_url`。

示例：

```text
POST http://127.0.0.1:8000/detect/fldcf
Content-Type: application/json
```

后端发送的请求体：

```json
{
  "image_url": "http://localhost:5021/files/user_input/example.png",
  "level": 1,
  "algorithm": "FLDCF",
  "user_id": "00ae9601000000708000000000000002"
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `image_url` | string | 待检测图片的可访问 URL。算法服务需要能从自己的运行环境访问这个地址。 |
| `level` | number/null | 用户创建任务时选择的检测等级。算法不需要分级时可以忽略。 |
| `algorithm` | string/null | 算法模型名称，来自 `algo_models.name`。 |
| `user_id` | string/null | 用户 ID，使用无连字符的 GUID 字符串。算法不需要用户隔离时可以忽略。 |

## 算法响应 DTO

算法服务成功时必须返回 HTTP 2xx，并返回可被后端解析的 JSON。

后端当前反序列化目标是 `DetectionResult`：

```json
{
  "type": "splice",
  "isFake": true,
  "confidence": 0.93,
  "maskUrl": "http://127.0.0.1:8000/output/example-mask.png"
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `type` | string/null | 算法结果类型，当前后端保留但不参与核心流程。 |
| `isFake` | boolean/null | 是否判定为伪造。 |
| `confidence` | number/null | 置信度，建议范围为 `0` 到 `1`。 |
| `maskUrl` | string/null | 结果图、mask 图或可视化结果地址。后端会保存该地址并展示给前端。 |

注意：后端 C# 属性是 `IsFake`、`MaskUrl`，ASP.NET Core 默认 JSON 配置通常按 camelCase 解析，因此算法服务推荐返回 `isFake`、`maskUrl`。如需兼容性更高，也可以在联调时确认当前运行环境是否接受 `IsFake`、`MaskUrl`。

## 健康检查接口

后端会对启用的算法做健康检查。

规则：

- 如果 `api_url` 路径是 `/detect/{algorithm}`，后端会请求 `/health/{algorithm}`。
- 其他路径下，后端会直接请求 `api_url`。
- 健康检查使用 `GET`，2 秒超时。
- 返回 HTTP 2xx 即视为在线。

示例：

```text
GET http://127.0.0.1:8000/health/fldcf
```

健康检查响应体没有强制 DTO，建议返回简单 JSON：

```json
{
  "status": "ok",
  "algorithm": "FLDCF"
}
```

## 错误处理要求

算法服务应遵守以下约定：

- 检测成功返回 HTTP 2xx 和合法 JSON。
- 检测失败返回非 2xx，后端会把任务标记为失败。
- 避免返回 HTML、纯文本或无法解析的 JSON。
- 长耗时任务应在后端调度超时前完成；超时会被记录为 `algorithm request timeout`。
- `maskUrl` 如果是外部地址，前端和用户浏览器也需要能访问。

## 最小 FastAPI 示例

```python
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()


class DetectRequest(BaseModel):
    image_url: str
    level: int | None = None
    algorithm: str | None = None
    user_id: str | None = None


class DetectResponse(BaseModel):
    type: str | None = None
    isFake: bool | None = None
    confidence: float | None = None
    maskUrl: str | None = None


@app.post("/detect/fldcf", response_model=DetectResponse)
async def detect_fldcf(request: DetectRequest):
    return DetectResponse(
        type="tamper",
        isFake=True,
        confidence=0.93,
        maskUrl="http://127.0.0.1:8000/output/example-mask.png",
    )


@app.get("/health/fldcf")
async def health_fldcf():
    return {"status": "ok", "algorithm": "FLDCF"}
```
