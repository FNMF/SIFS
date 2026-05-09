export function formatTime(value) {
  if (!value) return '暂无'
  const normalized = typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value) && !/[zZ]|[+-]\d{2}:\d{2}$/.test(value)
    ? `${value}Z`
    : value
  const date = new Date(normalized)
  if (Number.isNaN(date.getTime())) return '暂无'
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
