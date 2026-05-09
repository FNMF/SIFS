export function formatTime(value) {
  if (!value) return '-'
  const normalized = typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value) && !/[zZ]|[+-]\d{2}:\d{2}$/.test(value)
    ? `${value}Z`
    : value
  const date = new Date(normalized)
  if (Number.isNaN(date.getTime())) return '-'
  return new Intl.DateTimeFormat('zh-CN', {
    timeZone: 'Asia/Shanghai',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  }).format(date)
}

export function formatDuration(value) {
  if (value === null || value === undefined || value === '') return '-'
  const seconds = Number(value)
  if (Number.isNaN(seconds)) return '-'
  if (seconds < 60) return `${seconds.toFixed(1)} 秒`
  return `${(seconds / 60).toFixed(1)} 分钟`
}

export function statusText(status) {
  return {
    online: '在线',
    offline: '离线',
    timeout: '超时',
    disabled: '停用',
    running: '运行中',
    queued: '等待中',
    pending: '等待中',
    done: '成功',
    success: '成功',
    failed: '失败',
    canceled: '已取消',
    deleted: '已删除'
  }[status] || status || '-'
}

export function tryParseJson(value, fallback = null) {
  if (!value) return fallback
  if (typeof value === 'object') return value
  try {
    return JSON.parse(value)
  } catch {
    return fallback
  }
}
