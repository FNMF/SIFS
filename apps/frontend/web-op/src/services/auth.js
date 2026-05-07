import { request } from './request'

export function loginApi(data) {
  return request('/api/identity/login', {
    method: 'POST',
    body: JSON.stringify(data)
  })
}

export function registerApi(data) {
  return request('/api/identity/register', {
    method: 'POST',
    body: JSON.stringify(data)
  })
}

export function refreshTokenApi(refreshToken) {
  return request('/api/identity/refresh', {
    method: 'POST',
    body: JSON.stringify(refreshToken)
  }, {
    skipAutoRefresh: true
  })
}