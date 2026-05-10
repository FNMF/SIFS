import { request } from './request'

export function toQuery(params = {}) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    if (Array.isArray(value)) {
      value.forEach((item) => search.append(key, item))
      return
    }
    search.set(key, value)
  })
  const query = search.toString()
  return query ? `?${query}` : ''
}

export const dashboardApi = {
  summary: () => request('/api/admin/dashboard/summary'),
  recentTasks: (params) => request(`/api/admin/dashboard/recent-tasks${toQuery(params)}`),
  recentLogs: (params) => request(`/api/admin/dashboard/recent-logs${toQuery(params)}`),
  taskStatusCount: () => request('/api/admin/dashboard/task-status-count'),
  algoStatusCount: () => request('/api/admin/dashboard/algo-status-count'),
  algoHealth: (params) => request(`/api/admin/dashboard/algo-health${toQuery(params)}`)
}

export const algoApi = {
  list: (params) => request(`/api/admin/algos${toQuery(params)}`),
  get: (id) => request(`/api/admin/algos/${id}`),
  create: (data) => request('/api/admin/algos', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => request(`/api/admin/algos/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  enable: (id) => request(`/api/admin/algos/${id}/enable`, { method: 'POST' }),
  disable: (id) => request(`/api/admin/algos/${id}/disable`, { method: 'POST' }),
  remove: (id) => request(`/api/admin/algos/${id}`, { method: 'DELETE' })
}

export const taskApi = {
  list: (params) => request(`/api/admin/tasks${toQuery(params)}`),
  detail: (id) => request(`/api/admin/tasks/${id}`),
  statusFlow: (id) => request(`/api/admin/tasks/${id}/status-flow`),
  cancel: (id) => request(`/api/admin/tasks/${id}/cancel`, { method: 'POST' }),
  retry: (id) => request(`/api/admin/tasks/${id}/retry`, { method: 'POST' }),
  remove: (id) => request(`/api/admin/tasks/${id}`, { method: 'DELETE' })
}

export const operationLogApi = {
  list: (params) => request(`/api/admin/operation-logs${toQuery(params)}`)
}

export const rbacApi = {
  me: () => request('/api/admin/rbac/me', {}, { silent: true }),
  myPermissions: () => request('/api/admin/rbac/permissions/me', {}, { silent: true }),
  adminCheck: () => request('/api/admin/rbac/roles/admin-check', {}, { silent: true }),
  roles: () => request('/api/admin/rbac/roles'),
  users: (params) => request(`/api/admin/users${toQuery(params)}`),
  userRoles: (id) => request(`/api/admin/rbac/users/${id}/roles`),
  setUserRoles: (id, roles) => request(`/api/admin/rbac/users/${id}/roles`, {
    method: 'POST',
    body: JSON.stringify({ roles })
  }),
  userPermissions: (id) => request(`/api/admin/rbac/users/${id}/permissions`)
}
