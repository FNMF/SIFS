import { request } from './request'

export function loginApi(payload) {
  return request('/api/identity/login', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function registerApi(payload) {
  return request('/api/identity/register', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function refreshTokenApi(refreshToken) {
  return request('/api/identity/refresh', {
    method: 'POST',
    body: JSON.stringify(refreshToken)
  })
}
