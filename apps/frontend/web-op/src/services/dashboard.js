import { request } from './request'

function toQuery(params = {}) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, value)
    }
  })
  const query = search.toString()
  return query ? `?${query}` : ''
}

export function getDashboardSummary() {
  return request('/api/admin/dashboard/summary')
}

export function getRecentTasks(params = {}) {
  return request(`/api/admin/dashboard/recent-tasks${toQuery(params)}`)
}

export function getRecentLogs(params = {}) {
  return request(`/api/admin/dashboard/recent-logs${toQuery(params)}`)
}

export function getTaskStatusCount() {
  return request('/api/admin/dashboard/task-status-count')
}

export function getAlgoStatusCount() {
  return request('/api/admin/dashboard/algo-status-count')
}

export function getAlgoHealth(params = {}) {
  return request(`/api/admin/dashboard/algo-health${toQuery(params)}`)
}
