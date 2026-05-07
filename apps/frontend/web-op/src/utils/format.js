export function formatTime(value) {
  return value ? new Date(value).toLocaleString() : '-'
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
